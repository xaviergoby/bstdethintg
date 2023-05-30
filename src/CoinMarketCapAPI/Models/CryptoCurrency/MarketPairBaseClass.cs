using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class MarketPairBaseClass
{
    [JsonPropertyName("currency_id")]
    public long CurrencyId { get; set; }

    [JsonPropertyName("currency_symbol")]
    public string CurrencySymbol { get; set; }

    [JsonPropertyName("currency_type")]
    public string CurrencyType { get; set; }
}