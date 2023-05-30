using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class OhlcvHistoricalQuotes
{
    [JsonPropertyName("time_open")]
    public DateTimeOffset TimeOpen { get; set; }

    [JsonPropertyName("time_close")]
    public DateTimeOffset TimeClose { get; set; }

    [JsonPropertyName("quote")]
    public Dictionary<string, OhlcvQuote> Quote { get; set; }
}