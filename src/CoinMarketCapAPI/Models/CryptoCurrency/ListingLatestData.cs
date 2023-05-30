using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class ListingLatestData : LastestDataDetail
{
    [JsonPropertyName("circulating_supply")]
    public decimal? CirculatingSupply { get; set; }

    [JsonPropertyName("total_supply")]
    public decimal? TotalSupply { get; set; }

    [JsonPropertyName("quote")]
    public Dictionary<string, Quote> Quote { get; set; }
}
public class LastestDataDetail : CryptoCurrencyDetail
{
    [JsonPropertyName("max_supply")]
    public decimal? MaxSupply { get; set; }

    [JsonPropertyName("date_added")]
    public DateTimeOffset DateAdded { get; set; }

    [JsonPropertyName("num_market_pairs")]
    public long NumMarketPairs { get; set; }

    [JsonPropertyName("cmc_rank")]
    public long CmcRank { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset LastUpdated { get; set; }
}