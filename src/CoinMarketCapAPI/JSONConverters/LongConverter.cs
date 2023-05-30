using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.JSONConverters;

public class LongConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.True => 1L,
            JsonTokenType.False => 0L,
            JsonTokenType.String => long.TryParse(reader.GetString(), out var b) ? b : throw new JsonException(),
            JsonTokenType.Number => reader.TryGetInt64(out long l) ? l : throw new JsonException(),
            JsonTokenType.Null => 0,
            _ => throw new JsonException(),
        };

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}