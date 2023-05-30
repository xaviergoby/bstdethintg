using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.Exchange;

public class QuotesLatestData
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    [JsonPropertyName("num_market_pairs")]
    public long NumMarketPairs { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset LastUpdated { get; set; }

    [JsonPropertyName("quote")]
    public Dictionary<string, QuotesLatestQuote> Quote { get; set; }
}