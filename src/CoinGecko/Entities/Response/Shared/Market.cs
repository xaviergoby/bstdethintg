using System.Text.Json.Serialization;

namespace CoinGeckoAPI.Entities.Response.Shared;

public class Market
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("identifier")]
    public string Identifier { get; set; }

    [JsonPropertyName("has_trading_incentive")]
    public bool HasTradingIncentive { get; set; }
}