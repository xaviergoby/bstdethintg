using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.Exchange;

public class ListingsHistoricalData
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    [JsonPropertyName("cmc_rank")]
    public long CmcRank { get; set; }

    [JsonPropertyName("num_market_pairs")]
    public long NumMarketPairs { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("quote")]
    public Dictionary<string, ListingsHistoricalQuote> Quote { get; set; }
}