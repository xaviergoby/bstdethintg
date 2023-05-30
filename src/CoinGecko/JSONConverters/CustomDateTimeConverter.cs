using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoinGeckoAPI.JSONConverters;

public class CoinGeckoDateConverter : JsonConverter<DateTimeOffset>
{
    private readonly string _format = "yyyy-MM-dd";

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DateTimeOffset.ParseExact(reader.GetString()!, _format, CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, DateTimeOffset dateTimeValue, JsonSerializerOptions options) =>
        writer.WriteStringValue(dateTimeValue.ToString(_format, CultureInfo.InvariantCulture));
}