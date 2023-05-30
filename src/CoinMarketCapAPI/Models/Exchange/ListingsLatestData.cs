using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.Exchange;

public class ListingsLatestData : CryptoCurrencyBase
{
    [JsonPropertyName("num_market_pairs")]
    public long NumMarketPairs { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset LastUpdated { get; set; }

    [JsonPropertyName("quote")]
    public Dictionary<string, ListingsLatestQuote> Quote { get; set; }
}