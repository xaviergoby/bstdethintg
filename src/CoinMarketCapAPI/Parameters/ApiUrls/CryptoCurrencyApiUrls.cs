using CoinMarketCapAPI.Services;

namespace CoinMarketCapAPI;

public class CryptoCurrencyApiUrls
{
    private const string CryptoCurrencyV1Path = "v1/cryptocurrency";
    private const string CryptoCurrencyV2Path = "v2/cryptocurrency";

    public static Uri MetadataUri(long[] id, string[] symbol, string address)
    {
        return QueryStringService.CreateUrl($"{CryptoCurrencyV2Path}/info", new Dictionary<string, object>
        {
            {"id", string.Join(",", id)},
            {"symbol", string.Join(",", symbol)},
            {"address", address}
        });
    }

    public static Uri IdMapUri(string listingStatus, int? start, int? limit, string sort, string[] symbol)
    {
        return QueryStringService.CreateUrl($"{CryptoCurrencyV1Path}/map", new Dictionary<string, object>
        {
            {"listing_status", listingStatus},
            {"start", start},
            {"limit", limit},
            {"sort", sort},
            {"symbol", string.Join(",", symbol)}
        });
    }

    public static Uri ListingLatestUri(int? start, int? limit, string[] convert, string sort, string sortDir,
        string cryptocurrencyType)
    {
        return QueryStringService.CreateUrl($"{CryptoCurrencyV1Path}/listings/latest"
            , new Dictionary<string, object>
            {
                {"start", start},
                {"limit", limit},
                {"convert", string.Join(",", convert)},
                {"sort", sort},
                {"sort_dir", sortDir},
                {"cryptocurrency_type", cryptocurrencyType}
            });
    }

    public static Uri ListingHistoricalUri(string timeStamp, int? start, int? limit, string[] convert, string sort,
        string sortDir, string cryptoCurrencyType)
    {
        return QueryStringService.CreateUrl($"{CryptoCurrencyV1Path}/listings/historical",
            new Dictionary<string, object>
            {
                {"timeStamp", timeStamp},
                {"start", start},
                {"limit", limit},
                {"convert", string.Join(",", convert)},
                {"sort", sort},
                {"sort_dir", sortDir},
                {"cryptocurrency_type", cryptoCurrencyType}
            });
    }

    public static Uri LastestMarketPairsUri(long? id, string symbol, int? start, int? limit, string[] convert)
    {
        return QueryStringService.CreateUrl($"{CryptoCurrencyV2Path}/market-pairs/latest"
            , new Dictionary<string, object>
            {
                {"id", id},
                {"symbol", symbol},
                {"start", start},
                {"limit", limit},
                {"convert", string.Join(",", convert)}
            });
    }

    public static Uri HistoricalOhlcvUri(long? id, string symbol, string timePeriod, string timeStart,
        string timeEnd,
        int? count, string interval, string[] convert)
    {
        return QueryStringService.CreateUrl($"{CryptoCurrencyV2Path}/ohlcv/historical"
            , new Dictionary<string, object>
            {
                {"id", id},
                {"symbol", symbol},
                {"time_period", timePeriod},
                {"time_start", timeStart},
                {"time_end", timeEnd},
                {"count", count},
                {"interval", interval},
                {"convert", string.Join(",", convert)}
            });
    }

    public static Uri LatestOhlcvUri(long[] id, string[] symbol, string[] convert)
    {
        return QueryStringService.CreateUrl($"{CryptoCurrencyV2Path}/ohlcv/latest"
            , new Dictionary<string, object>
            {
                {"id", string.Join(",", id)},
                {"symbol", string.Join(",", symbol)},
                {"convert", string.Join(",", convert)}
            });
    }

    public static Uri HistoricalQuotesUri(long? id, string symbol, string timeStart, string timeEnd, int? count,
        string interval, string[] convert)
    {
        return QueryStringService.CreateUrl($"{CryptoCurrencyV2Path}/quotes/historical",
            new Dictionary<string, object>
            {
                {"id", id},
                {"symbol", symbol},
                {"time_start", timeStart},
                {"time_end", timeEnd},
                {"count", count},
                {"interval", interval},
                {"convert", string.Join(",", convert)}
            });
    }

    public static Uri LatestQuotesUri(long[] id, string[] symbol, string[] convert)
    {
        return QueryStringService.CreateUrl($"{CryptoCurrencyV2Path}/quotes/latest", new Dictionary<string, object>
        {
            {"id", string.Join("," ,id)},
            {"symbol", string.Join(",", symbol)},
            {"convert", string.Join(",", convert)}
        });
    }
}