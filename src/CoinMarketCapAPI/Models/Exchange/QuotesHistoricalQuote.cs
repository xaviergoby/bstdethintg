using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.Exchange;

public class QuotesHistoricalQuote
{
    [JsonPropertyName("volume_24h")]
    public decimal Volume24H { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }
}