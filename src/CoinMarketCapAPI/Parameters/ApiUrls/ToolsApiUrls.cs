using CoinMarketCapAPI.Services;

namespace CoinMarketCapAPI;

public class ToolsApiUrls
{
    private const string ToolsApiV2Path = "v2/tools";

    public static Uri InfoUri(float amount, string id, string symbol,
        string time, string[] convert)
    {
        return QueryStringService.CreateUrl($"{ToolsApiV2Path}/price-conversion", new Dictionary<string, object>
        {
            {"amount", amount},
            {"id", id},
            {"symbol", symbol},
            {"time", time},
            {"convert", string.Join(",", convert)}
        });
    }
}