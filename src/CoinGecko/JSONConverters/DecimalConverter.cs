using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoinGeckoAPI.JSONConverters;

public class DecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.True => 1,
            JsonTokenType.False => 0,
            JsonTokenType.String => decimal.TryParse(reader.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var b) ? b : throw new JsonException(),
            JsonTokenType.Number => reader.TryGetDecimal(out decimal l) ? l : throw new JsonException(),
            JsonTokenType.Null => 0,
            _ => throw new JsonException(),
        };

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

public class NullableDecimalConverter : JsonConverter<decimal?>
{
    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.True => 1,
            JsonTokenType.False => 0,
            JsonTokenType.String => decimal.TryParse(reader.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var b) ? b : throw new JsonException(),
            JsonTokenType.Number => reader.TryGetDecimal(out decimal l) ? l : throw new JsonException(),
            JsonTokenType.Null => null,
            _ => throw new JsonException(),
        };

    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value != null) writer.WriteNumberValue((decimal)value);
    }
}