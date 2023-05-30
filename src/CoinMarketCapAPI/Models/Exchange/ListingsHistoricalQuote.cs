using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.Exchange;

public class ListingsHistoricalQuote
{
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("volume_24h")]
    public decimal Volume24H { get; set; }

    [JsonPropertyName("volume_7d")]
    public decimal Volume7D { get; set; }

    [JsonPropertyName("volume_30d")]
    public decimal Volume30D { get; set; }

    [JsonPropertyName("percent_change_volume_24h")]
    public decimal PercentChangeVolume24H { get; set; }

    [JsonPropertyName("percent_change_volume_7d")]
    public decimal PercentChangeVolume7D { get; set; }

    [JsonPropertyName("percent_change_volume_30d")]
    public decimal PercentChangeVolume30D { get; set; }
}