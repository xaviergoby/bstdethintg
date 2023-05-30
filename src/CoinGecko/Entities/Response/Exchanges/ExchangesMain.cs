using System.Text.Json.Serialization;

namespace CoinGeckoAPI.Entities.Response.Exchanges;

public class ExchangesMain
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("has_trading_incentive")]
    public bool? HasTradingIncentive { get; set; }

    [JsonPropertyName("trust_score")]
    public decimal? TrustScore { get; set; }

    [JsonPropertyName("trust_score_rank")]
    public decimal? TrustScoreRank { get; set; }

    [JsonPropertyName("trade_volume_24h_btc")]
    public decimal? TradeVolume24HBtc { get; set; }

    [JsonPropertyName("trade_volume_24h_btc_normalized")]
    public decimal? TradeVolume24HBtcNormalized { get; set; }
}