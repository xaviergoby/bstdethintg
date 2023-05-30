using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class CryptoCurrencyDetail : CryptoCurrencyBase
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }
}