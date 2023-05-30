using System.Text.Json.Serialization;

namespace CoinGeckoAPI.Entities.Response.Coins;

public class MarketChartById
{
    [JsonPropertyName("prices")]
    public decimal?[][] Prices { get; set; }

    [JsonPropertyName("market_caps")]
    public decimal?[][] MarketCaps { get; set; }

    [JsonPropertyName("total_volumes")]
    public decimal?[][] TotalVolumes { get; set; }
}