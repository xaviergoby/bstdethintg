using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class OhlcvLatestData
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset LastUpdated { get; set; }

    [JsonPropertyName("time_open")]
    public DateTimeOffset TimeOpen { get; set; }

    [JsonPropertyName("time_close")]
    public object TimeClose { get; set; }

    [JsonPropertyName("quote")]
    public Dictionary<string, OhlcvQuote> Quote { get; set; }
}