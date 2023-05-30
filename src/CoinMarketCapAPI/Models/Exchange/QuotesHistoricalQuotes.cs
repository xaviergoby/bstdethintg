using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.Exchange;

public class QuotesHistoricalQuotes
{
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("quote")]
    public QuotesHistoricalQuote Quote { get; set; }

    [JsonPropertyName("num_market_pairs")]
    public long NumMarketPairs { get; set; }
}