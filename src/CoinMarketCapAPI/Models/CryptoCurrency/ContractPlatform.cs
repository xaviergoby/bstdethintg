using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class ContractPlatform
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("coin")]
    public CryptoCurrencyDetail Coin { get; set; }
}
