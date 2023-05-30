using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class OhlcvQuote
{
    [JsonPropertyName("open")]
    public decimal Open { get; set; }

    [JsonPropertyName("high")]
    public decimal High { get; set; }

    [JsonPropertyName("low")]
    public decimal Low { get; set; }

    [JsonPropertyName("close")]
    public decimal Close { get; set; }

    [JsonPropertyName("volume")]
    public decimal Volume { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }
}