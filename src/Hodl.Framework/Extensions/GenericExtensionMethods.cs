using System.Text.Json;

namespace Hodl.Extensions;

public static class GenericExtensionMethods
{
    public static T GetValueOrDefault<T>(this T self, T defaultValue)
    {
        return self == null ? defaultValue : self;
    }

    public static T DeepCopy<T>(this T self)
    {
        var serialized = JsonSerializer.Serialize(self);
        return JsonSerializer.Deserialize<T>(serialized);
    }
}