using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.Exchange;

public class InfoData : CryptoCurrencyBase
{
    [JsonPropertyName("urls")]
    public Urls Urls { get; set; }

    [JsonPropertyName("logo")]
    public Uri Logo { get; set; }
}