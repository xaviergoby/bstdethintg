using CoinMarketCapAPI.Models;
using CoinMarketCapAPI.Models.CryptoCurrency;
using CoinMarketCapAPI.Parameters;
using System.Text.Json;

namespace CoinMarketCapAPI.Clients;

public class CryptoCurrencyClient : BaseApiClient, ICryptoCurrencyClient
{
    public CryptoCurrencyClient(HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions) :
        base(httpClient, jsonSerializerOptions)
    {
    }

    public async Task<ResponseMain<Dictionary<string, CryptoCurrencyInfoData[]>>> GetMetaData(string[] symbols)
    {
        return await GetAsync<ResponseMain<Dictionary<string, CryptoCurrencyInfoData[]>>>(
                CryptoCurrencyApiUrls.MetadataUri(Array.Empty<long>(), symbols, string.Empty))
            .ConfigureAwait(false);
    }

    public async Task<ResponseMain<Dictionary<long, CryptoCurrencyInfoData>>> GetMetaData(long[] ids)
    {
        return await GetAsync<ResponseMain<Dictionary<long, CryptoCurrencyInfoData>>>(
                CryptoCurrencyApiUrls.MetadataUri(ids, Array.Empty<string>(), string.Empty))
            .ConfigureAwait(false);
    }

    public async Task<ResponseMain<IdMapData[]>> GetIdMap(string listingStatus, int? start, int? limit,
        string sort, string[] symbols)
    {
        return await GetAsync<ResponseMain<IdMapData[]>>(
                CryptoCurrencyApiUrls.IdMapUri(listingStatus, start, limit, sort, symbols))
            .ConfigureAwait(false);
    }

    public async Task<ResponseMain<IdMapData[]>> GetIdMap(int? limit, string sort)
    {
        return await GetAsync<ResponseMain<IdMapData[]>>(
                CryptoCurrencyApiUrls.IdMapUri(ListingStatus.Active, 1, limit, sort, new[] { string.Empty }))
            .ConfigureAwait(false);
    }

    public async Task<ResponseMain<IdMapData[]>> GetIdMap(string[] symbols)
    {
        return await GetAsync<ResponseMain<IdMapData[]>>(
                CryptoCurrencyApiUrls.IdMapUri(ListingStatus.Active, 1, limit: null, sort: SortField.Id, symbol: symbols))
            .ConfigureAwait(false);
    }

    public async Task<ResponseMain<ListingHistoricalData[]>> GetListingsHistorical(string timestamp, int? start,
        int? limit, string[] convert, string sortField, string sortDirection,
        string cryptocurrencyType)
    {
        return await GetAsync<ResponseMain<ListingHistoricalData[]>>(
            CryptoCurrencyApiUrls.ListingHistoricalUri(timestamp, start, limit, convert, sortField, sortDirection,
                cryptocurrencyType)).ConfigureAwait(false);
    }

    public async Task<ResponseMain<ListingHistoricalData[]>> GetListingsHistorical(string timestamp, string[] convert)
    {
        return await GetAsync<ResponseMain<ListingHistoricalData[]>>(
            CryptoCurrencyApiUrls.ListingHistoricalUri(timestamp, null, null, convert, SortField.MarketCap, SortDirection.Desc,
                CryptoCurrencyType.All)).ConfigureAwait(false);
    }

    public async Task<ResponseMain<ListingLatestData[]>> GetListingLatest(int? start, int? limit, string[] convert,
        string sortField, string sortDir,
        string cryptoCurrencyType)
    {
        return await GetAsync<ResponseMain<ListingLatestData[]>>(CryptoCurrencyApiUrls.ListingLatestUri(start,
            limit, convert, sortField, sortDir, cryptoCurrencyType)).ConfigureAwait(false);
    }

    public async Task<ResponseMain<ListingLatestData[]>> GetListingLatest()
    {
        return await GetAsync<ResponseMain<ListingLatestData[]>>(CryptoCurrencyApiUrls.ListingLatestUri(null,
            null, new[] { string.Empty }, string.Empty, string.Empty, string.Empty)).ConfigureAwait(false);
    }

    public async Task<ResponseMain<MarketPairsLatestData>> GetMarketPairLatest(string symbol, int? start, int? limit, string[] convert)
    {
        return await GetAsync<ResponseMain<MarketPairsLatestData>>(
            CryptoCurrencyApiUrls.LastestMarketPairsUri(null, symbol, start, limit, convert)).ConfigureAwait(false);
    }

    public async Task<ResponseMain<MarketPairsLatestData>> GetMarketPairLatest(long id, int? start, int? limit, string[] convert)
    {
        return await GetAsync<ResponseMain<MarketPairsLatestData>>(
            CryptoCurrencyApiUrls.LastestMarketPairsUri(id, string.Empty, start, limit, convert)).ConfigureAwait(false);
    }

    public async Task<ResponseMain<OhlcvHistoricalData>> GetOhlvcHistorical(string symbol,
        string timePeriod, string timeStart, string timeEnd, int? count, string interval, string[] convert)
    {
        return await GetAsync<ResponseMain<OhlcvHistoricalData>>(CryptoCurrencyApiUrls.HistoricalOhlcvUri(null,
            symbol, timePeriod, timeStart, timeEnd, count, interval, convert))
            .ConfigureAwait(false);
    }

    public async Task<ResponseMain<OhlcvHistoricalData>> GetOhlvcHistorical(long id,
        string timePeriod, string timeStart, string timeEnd, int? count, string interval, string[] convert)
    {
        return await GetAsync<ResponseMain<OhlcvHistoricalData>>(CryptoCurrencyApiUrls.HistoricalOhlcvUri(id,
            string.Empty, timePeriod, timeStart, timeEnd, count, interval, convert))
            .ConfigureAwait(false);
    }


    public async Task<ResponseMain<Dictionary<string, OhlcvLatestData>>> GetOhlcvLatest(string[] symbols, string[] convert)
    {
        return await GetAsync<ResponseMain<Dictionary<string, OhlcvLatestData>>>(
            CryptoCurrencyApiUrls.LatestOhlcvUri(Array.Empty<long>(), symbols, convert)).ConfigureAwait(false);
    }

    public async Task<ResponseMain<Dictionary<long, OhlcvLatestData>>> GetOhlcvLatest(long[] ids, string[] convert)
    {
        return await GetAsync<ResponseMain<Dictionary<long, OhlcvLatestData>>>(
            CryptoCurrencyApiUrls.LatestOhlcvUri(ids, Array.Empty<string>(), convert)).ConfigureAwait(false);
    }

    public async Task<ResponseMain<QuotesHistoricalData>> GetQuotesHistorical(long id,
        string timeStart, string timeEnd, int? count, string interval, string[] convert)
    {
        return await GetAsync<ResponseMain<QuotesHistoricalData>>(
                CryptoCurrencyApiUrls.HistoricalQuotesUri(id, string.Empty, timeStart, timeEnd, count, interval, convert))
            .ConfigureAwait(false);
    }

    public async Task<ResponseMain<QuotesHistoricalData>> GetQuotesHistorical(string symbol,
        string timeStart, string timeEnd, int? count, string interval, string[] convert)
    {
        return await GetAsync<ResponseMain<QuotesHistoricalData>>(
                CryptoCurrencyApiUrls.HistoricalQuotesUri(null, symbol, timeStart, timeEnd, count, interval, convert))
            .ConfigureAwait(false);
    }

    public async Task<ResponseMain<Dictionary<string, QuotesLatestData>>> GetQuotesLatest(string[] symbols, string[] convert)
    {
        return await GetAsync<ResponseMain<Dictionary<string, QuotesLatestData>>>(
            CryptoCurrencyApiUrls.LatestQuotesUri(Array.Empty<long>(), symbols, convert));
    }

    public async Task<ResponseMain<Dictionary<long, QuotesLatestData>>> GetQuotesLatest(long[] ids, string[] convert)
    {
        return await GetAsync<ResponseMain<Dictionary<long, QuotesLatestData>>>(
            CryptoCurrencyApiUrls.LatestQuotesUri(ids, Array.Empty<string>(), convert));
    }
}