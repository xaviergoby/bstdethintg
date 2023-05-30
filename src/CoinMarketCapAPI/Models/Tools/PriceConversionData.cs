using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.Tools;

public class PriceConversionData
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset LastUpdated { get; set; }

    [JsonPropertyName("quote")]
    public Dictionary<string, PriceConversionQuote> Quote { get; set; }
}