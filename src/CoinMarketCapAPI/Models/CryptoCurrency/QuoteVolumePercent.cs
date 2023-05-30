using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class QuoteVolumePercent
{
    [JsonPropertyName("volume_24h")]
    public decimal? Volume24H { get; set; }

    [JsonPropertyName("percent_change_1h")]
    public decimal? PercentChange1H { get; set; }

    [JsonPropertyName("percent_change_24h")]
    public decimal? PercentChange24H { get; set; }

    [JsonPropertyName("percent_change_7d")]
    public decimal? PercentChange7D { get; set; }
}