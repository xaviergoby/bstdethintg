using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class QuotesLatestData : LastestDataDetail
{
    [JsonPropertyName("circulating_supply")]
    public decimal CirculatingSupply { get; set; }

    [JsonPropertyName("total_supply")]
    public decimal TotalSupply { get; set; }

    [JsonPropertyName("quote")]
    public Dictionary<string, QuotesLatestQuote> Quote { get; set; }
}