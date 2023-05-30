using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class ListingHistoricalData : CryptoCurrencyDetail
{
    [JsonPropertyName("cmc_rank")]
    public long CmcRank { get; set; }

    [JsonPropertyName("num_market_pairs")]
    public long NumMarketPairs { get; set; }

    [JsonPropertyName("circulating_supply")]
    public decimal CirculatingSupply { get; set; }

    [JsonPropertyName("total_supply")]
    public decimal TotalSupply { get; set; }

    [JsonPropertyName("max_supply")]
    public decimal MaxSupply { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset LastUpdated { get; set; }

    [JsonPropertyName("quote")]
    public Dictionary<string, Quote> Quote { get; set; }
}