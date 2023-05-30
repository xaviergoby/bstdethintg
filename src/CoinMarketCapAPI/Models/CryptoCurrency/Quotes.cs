using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class Quotes
{
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("quote")]
    public Dictionary<string, QuotesHistoricalQuote> Quote { get; set; }
}