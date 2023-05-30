using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

namespace Hodl.Api.Utils.Security;

internal class ConfigureJsonOptions : IConfigureOptions<JsonOptions>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ConfigureJsonOptions(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Configure(JsonOptions options)
    {
        // serialize enums as strings in api responses (e.g. Role)
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new RoleBasedJsonConverterFactory(_httpContextAccessor));
        // ignore omitted parameters on models to enable optional params (e.g. User update)
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        // Prevent the loop reference
        //x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    }
}