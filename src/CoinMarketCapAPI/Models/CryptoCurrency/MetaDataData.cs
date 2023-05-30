using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class CryptoCurrencyInfoData : CryptoCurrencyDetail
{
    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("logo")]
    public Uri Logo { get; set; }

    [JsonPropertyName("tags")]
    public string[] Tags { get; set; }

    [JsonPropertyName("urls")]
    public Urls Urls { get; set; }

    [JsonPropertyName("notice")]
    public string Notice { get; set; }

    [JsonPropertyName("date_added")]
    public DateTimeOffset? DateAdded { get; set; }

    [JsonPropertyName("date_launched")]
    public DateTimeOffset? DateLaunched { get; set; }

    [JsonPropertyName("platform")]
    public Platform Platform { get; set; }

    [JsonPropertyName("contract_address")]
    public ContractAddress[] ContractAddresses { get; set; }
}