using System.Text.Json.Serialization;

namespace CoinGeckoAPI.Entities.Response.Derivatives;

public class Derivatives
{
    [JsonPropertyName("market")]
    public string Market { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("coin_id")]
    public string CoinId { get; set; }

    [JsonPropertyName("index_id")]
    public string IndexId { get; set; }

    [JsonPropertyName("price")]
    public string Price { get; set; }

    [JsonPropertyName("price_percentage_change_24h")]
    public decimal? PricePercentageChange24H { get; set; }

    [JsonPropertyName("contract_type")]
    public string ContractType { get; set; }

    [JsonPropertyName("index")]
    public decimal? Index { get; set; }

    [JsonPropertyName("basis")]
    public decimal? Basis { get; set; }

    [JsonPropertyName("spread")]
    public decimal? Spread { get; set; }

    [JsonPropertyName("funding_rate")]
    public decimal? FundingRate { get; set; }

    [JsonPropertyName("open_interest")]
    public decimal? OpenInterest { get; set; }

    [JsonPropertyName("volume_24h")]
    public decimal? Volume24H { get; set; }

    [JsonPropertyName("last_traded_at")]
    public long LastTradedAt { get; set; }

    [JsonPropertyName("expired_at")]
    public long ExpiredAt { get; set; }
}