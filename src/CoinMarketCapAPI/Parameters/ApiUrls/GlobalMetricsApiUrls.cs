using CoinMarketCapAPI.Services;

namespace CoinMarketCapAPI;

public class GlobalMetricsApiUrls
{
    private const string GlobalMetricApiV1Path = "v1/global-metrics";

    public static Uri QuotesHistoricalUri(string timeStart, string timeEnd, int? count, string interval,
        string[] convert)
    {
        return QueryStringService.CreateUrl($"{GlobalMetricApiV1Path}/quotes/historical",
            new Dictionary<string, object>
            {
                {"time_start", timeStart},
                {"time_end", timeEnd},
                {"count", count},
                {"interval", interval},
                {"convert", string.Join(",", convert)}
            });
    }

    public static Uri QuotesLatestUri(string[] convert)
    {
        return QueryStringService.CreateUrl($"{GlobalMetricApiV1Path}/quotes/latest", new Dictionary<string, object>
        {
            {"convert", string.Join(",", convert)}
        });
    }
}