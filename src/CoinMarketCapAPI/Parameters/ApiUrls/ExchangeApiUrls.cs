using CoinMarketCapAPI.Services;

namespace CoinMarketCapAPI;

public class ExchangeApiUrls
{
    private const string ExchangeApiV1Path = "v1/exchange";

    public static Uri InfoUri(string id, string slug)
    {
        return QueryStringService.CreateUrl($"{ExchangeApiV1Path}/info", new Dictionary<string, object>
        {
            {"id", id},
            {"slug", slug}
        });
    }

    public static Uri MapUri(string listingStatus, string slug, int? start, int? limit)
    {
        return QueryStringService.CreateUrl($"{ExchangeApiV1Path}/map", new Dictionary<string, object>
        {
            {"listing_status", listingStatus},
            {"slug", slug},
            {"start", start},
            {"limit", limit}
        });
    }

    public static Uri ListingsHistorical(string timeStamp, int start, int limit, string sortField, string sortDir,
        string marketType, string[] convert)
    {
        return QueryStringService.CreateUrl($"{ExchangeApiV1Path}/listings/historical", new Dictionary<string, object>
        {
            {"timestamp", timeStamp},
            {"start", start},
            {"limit", limit},
            {"sort", sortField},
            {"sort_dir", sortDir},
            {"market_type", marketType},
            {"convert", string.Join(",", convert)}
        });
    }

    public static Uri ListingsLatest(int? start, int? limit, string sortField, string sortDir, string marketType,
        string[] convert)
    {
        return QueryStringService.CreateUrl($"{ExchangeApiV1Path}/listings/latest", new Dictionary<string, object>
        {
            {"start", start},
            {"limit", limit},
            {"sort", sortField},
            {"sort_dir", sortDir},
            {"market_type", marketType},
            {"convert", string.Join(",", convert)}
        });
    }

    public static Uri MarketPairsLatest(string id, string slug, int? start, int? limit, string[] convert)
    {
        return QueryStringService.CreateUrl($"{ExchangeApiV1Path}/market-pairs/latest", new Dictionary<string, object>
        {
            {"id", id},
            {"slug", slug},
            {"start", start},
            {"limit", limit},
            {"convert", string.Join(",", convert)}
        });
    }

    public static Uri QuotesHistorical(string id, string slug, string timeStart, string timeEnd, int? count,
        string interval, string[] convert)
    {
        return QueryStringService.CreateUrl($"{ExchangeApiV1Path}/quotes/historical", new Dictionary<string, object>
        {
            {"id", id},
            {"slug", slug},
            {"time_start", timeStart},
            {"time_end", timeEnd},
            {"count", count},
            {"interval", interval},
            {"convert", string.Join(",", convert)}
        });
    }

    public static Uri QuotesLatest(long? id, string slug, string[] convert)
    {
        return QueryStringService.CreateUrl($"{ExchangeApiV1Path}/quotes/latest", new Dictionary<string, object>
        {
            {"id", id},
            {"slug", slug},
            {"convert", string.Join(",", convert)}
        });
    }
}