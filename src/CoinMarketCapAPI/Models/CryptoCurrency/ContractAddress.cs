using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class ContractAddress
{
    [JsonPropertyName("contract_address")]
    public string Address { get; set; }

    [JsonPropertyName("platform")]
    public ContractPlatform Platform { get; set; }
}
