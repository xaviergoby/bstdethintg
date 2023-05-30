using System.Text.Json.Serialization;

namespace CoinMarketCapAPI;

public class GlobalMetricsHistoricalQuote
{
    [JsonPropertyName("total_market_cap")]
    public decimal TotalMarketCap { get; set; }

    [JsonPropertyName("total_volume_24h")]
    public decimal TotalVolume24H { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }
}