using System.Text.Json.Serialization;

namespace CoinGeckoAPI.Entities.Response.Global;

public class Global
{
    [JsonPropertyName("data")]
    public GlobalData Data { get; set; }
}

public class GlobalData
{
    [JsonPropertyName("active_cryptocurrencies")]
    public long ActiveCryptocurrencies { get; set; }

    [JsonPropertyName("upcoming_icos")]
    public long UpcomingIcos { get; set; }

    [JsonPropertyName("ongoing_icos")]
    public long OngoingIcos { get; set; }

    [JsonPropertyName("ended_icos")]
    public long EndedIcos { get; set; }

    [JsonPropertyName("markets")]
    public long Markets { get; set; }

    [JsonPropertyName("total_market_cap")]
    public Dictionary<string, decimal> TotalMarketCap { get; set; }

    [JsonPropertyName("total_volume")]
    public Dictionary<string, decimal> TotalVolume { get; set; }

    [JsonPropertyName("market_cap_percentage")]
    public Dictionary<string, decimal> MarketCapPercentage { get; set; }

    [JsonPropertyName("updated_at")]
    public long UpdatedAt { get; set; }
}