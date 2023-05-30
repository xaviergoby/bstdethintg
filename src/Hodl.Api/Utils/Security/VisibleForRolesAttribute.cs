using System.Collections;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hodl.Api.Utils.Security;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class VisibleForRoles : Attribute
{
    private IEnumerable<string> _roles = null;

    public string Roles
    {
        get => _roles.ToString();
        set
        {
            _roles = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public IEnumerable<string> GetRoles() => _roles;
}

public class RoleBasedJsonConverterFactory : JsonConverterFactory
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RoleBasedJsonConverterFactory(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// This converter applies to all classes where the attribute 
    /// VisibleForRoles is used.
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <returns></returns>
    public override bool CanConvert(Type typeToConvert)
    {
        var propertiesToHide = typeToConvert.GetProperties()
            .Where(p => p.GetCustomAttribute<VisibleForRoles>() != null);

        return propertiesToHide.Any();
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        // Get all properties on the type that have a HideForAttribute attribute
        var propertiesToHide = typeToConvert.GetProperties()
            .Where(p => p.GetCustomAttribute<VisibleForRoles>() != null);

        Type classType = typeof(RoleBasedJsonConverter<>);
        Type[] typeParams = new Type[] { typeToConvert };
        Type constructedType = classType.MakeGenericType(typeParams);

        return Activator.CreateInstance(constructedType,
            _httpContextAccessor,
            propertiesToHide) as JsonConverter;
    }
}

public class RoleBasedJsonConverter<T> : JsonConverter<T>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEnumerable<PropertyInfo> _propertiesToCheck;

    public RoleBasedJsonConverter(
        IHttpContextAccessor httpContextAccessor,
        IEnumerable<PropertyInfo> propertiesToCheck)
    {
        _httpContextAccessor = httpContextAccessor;
        _propertiesToCheck = propertiesToCheck;
    }

    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsClass;

    /// <summary>
    /// Generate the object from the JSON string, it prevents the properties 
    /// protected by userroles to be set, when the user does not have the 
    /// required role assigned.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="JsonException"></exception>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // We only work with classes, so when only values are in the token, raise an error
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        //object instance = Activator.CreateInstance(_typeToConvert);
        object instance = Activator.CreateInstance(typeof(T));
        var userRoles = _httpContextAccessor.HttpContext.User
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value);

        return (T)ReadObject(ref reader, options, instance, userRoles);
    }

    /// <summary>
    /// Write the JSON output using the check for the user roles. When 
    /// properties are not protected, or the user is assigned the required 
    /// role, then the propertie values are added. When the user has no role 
    /// from the attribute assigned, a null value is returned in th property.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var userRoles = _httpContextAccessor.HttpContext.User
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value);

        // Here the output starts
        writer.WriteStartObject();

        foreach (var prop in value.GetType().GetProperties())
        {
            writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(prop.Name) ?? prop.Name);
            writer.WriteRawValue(CheckRoles(prop, userRoles)
                ? JsonSerializer.Serialize(prop.GetValue(value), options)
                : "null");
        }

        writer.WriteEndObject();
    }

    private bool CheckRoles(PropertyInfo prop, IEnumerable<string> userRoles)
    {
        if (prop == null)
            return false;

#if !DEBUG
        if (_propertiesToCheck.Contains(prop))
        {
            // Do the roles check here
            var propRoles = prop.GetCustomAttribute<VisibleForRoles>();

            return propRoles != null && userRoles != null &&
                propRoles.GetRoles().Intersect(userRoles).Any();
        }
#endif
        return true;
    }

    private object ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options, object instance, IEnumerable<string> userRoles)
    {
        var type = instance.GetType();
        var typeInfo = type.GetTypeInfo();
        var properties = typeInfo.GetProperties();

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            // Find the property on the instance, and check if we must 
            var propName = reader.GetString();
            var prop = properties.Where(p => p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            // Jump to the start of the value (always)
            reader.Read();

            if (prop != null && CheckRoles(prop, userRoles))
            {
                var propvalue = ReadValue(ref reader, options, prop.PropertyType, userRoles);
                prop.SetValue(instance, propvalue);
            }
        }

        return instance;
    }

    private object ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options, Type typeToReturn, IEnumerable<string> userRoles) =>
        reader.TokenType switch
        {
            JsonTokenType.StartObject => ReadObject(ref reader, options, Activator.CreateInstance(typeToReturn), userRoles),
            JsonTokenType.StartArray => ReadArray(ref reader, options, typeToReturn, userRoles),
            JsonTokenType.String or
            JsonTokenType.Number or
            JsonTokenType.True or
            JsonTokenType.False => JsonSerializer.Deserialize(ref reader, typeToReturn, options),
            _ => null
        };

    private object ReadArray(ref Utf8JsonReader reader, JsonSerializerOptions options, Type typeToReturn, IEnumerable<string> userRoles)
    {
        // Start collecting the array members
        return typeToReturn.IsArray
            ? ReturnArray(ref reader, options, typeToReturn, userRoles)
            : ReturnList(ref reader, options, typeToReturn, userRoles);
    }

    private object ReturnArray(ref Utf8JsonReader reader, JsonSerializerOptions options, Type typeToReturn, IEnumerable<string> userRoles)
    {
        Type itemType = typeToReturn.GetElementType();
        ArrayList list = new();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            list.Add(ReadValue(ref reader, options, itemType, userRoles));

        return list.ToArray(itemType);
    }

    private object ReturnList(ref Utf8JsonReader reader, JsonSerializerOptions options, Type typeToReturn, IEnumerable<string> userRoles)
    {
        Type itemType = typeToReturn.GetGenericArguments()[0];
        Type listType = typeof(List<>).MakeGenericType(new[] { itemType });
        IList list = (IList)Activator.CreateInstance(listType);

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            list.Add(ReadValue(ref reader, options, itemType, userRoles));

        return list;
    }
}