using CoinMarketCapAPI.Models;
using CoinMarketCapAPI.Models.Exchange;
using CoinMarketCapAPI.Parameters;
using System.Text.Json;

namespace CoinMarketCapAPI.Clients;

public class ExchangeClient : BaseApiClient, IExchangeClient
{
    public ExchangeClient(HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions) :
        base(httpClient, jsonSerializerOptions)
    {
    }

    public async Task<ResponseMain<Dictionary<string, InfoData>>> GetInfo(string id, string slug)
    {
        return await GetAsync<ResponseMain<Dictionary<string, InfoData>>>(ExchangeApiUrls.InfoUri(id, slug))
            .ConfigureAwait(false);
    }

    public async Task<ResponseMain<MapData[]>> GetMap(string listingStatus, string slug, int? start, int? limit)
    {
        return await GetAsync<ResponseMain<MapData[]>>(ExchangeApiUrls.MapUri(listingStatus, slug, start, limit))
            .ConfigureAwait(false);
    }

    public async Task<ResponseMain<ListingsHistoricalData[]>> GetListingHistorical(string timeStamp, int start,
        int limit, string sortField, string sortDir, string marketType,
        string[] convert)
    {
        return await GetAsync<ResponseMain<ListingsHistoricalData[]>>(
                ExchangeApiUrls.ListingsHistorical(timeStamp, start, limit, sortField, sortDir, marketType,
                    convert))
            .ConfigureAwait(false);
    }

    public async Task<ResponseMain<ListingsLatestData[]>> GetListingLatest(int? start, int? limit, string sortField,
        string sortDir, string marketType, string[] convert)
    {
        return await GetAsync<ResponseMain<ListingsLatestData[]>>(
            ExchangeApiUrls.ListingsLatest(start, limit, sortField, sortDir, marketType, convert));
    }

    public async Task<ResponseMain<ListingsLatestData[]>> GetListingLatest()
    {
        return await GetAsync<ResponseMain<ListingsLatestData[]>>(
                ExchangeApiUrls.ListingsLatest(null, null, SortField.Volume24H, SortDirection.Desc, MarketType.Fees,
                    new[] { string.Empty }))
            .ConfigureAwait(false);
    }

    public async Task<ResponseMain<MarketPairsLatestData>> GetMarketPairsLatest(string id, string slug, int? start,
        int? limit, string[] convert)
    {
        return await GetAsync<ResponseMain<MarketPairsLatestData>>(
            ExchangeApiUrls.MarketPairsLatest(id, slug, start, limit, convert)).ConfigureAwait(false);

    }

    public async Task<ResponseMain<MarketPairsLatestData>> GetMarketPairsLatest(string idOrSlug)
    {
        return int.TryParse(idOrSlug, out var id)
            ? await GetAsync<ResponseMain<MarketPairsLatestData>>(
                    ExchangeApiUrls.MarketPairsLatest(id.ToString(), string.Empty, null, null, new[] { string.Empty }))
                .ConfigureAwait(false)
            : await GetAsync<ResponseMain<MarketPairsLatestData>>(
                    ExchangeApiUrls.MarketPairsLatest(string.Empty, idOrSlug, null, null, new[] { string.Empty }))
                .ConfigureAwait(false);
    }

    public async Task<ResponseMain<QuotesHistoricalData>> GetQuotesHistorical(string id, string slug,
        string timeStart, string timeEnd, int count, string interval,
        string[] convert)
    {
        return await GetAsync<ResponseMain<QuotesHistoricalData>>(
                ExchangeApiUrls.QuotesHistorical(id, slug, timeStart, timeEnd, count, interval, convert))
            .ConfigureAwait(false);
    }

    public async Task<ResponseMain<QuotesHistoricalData>> GetQuotesHistorical(string idOrSlug, string timeStart, string timeEnd)
    {

        return int.TryParse(idOrSlug, out var id)
            ? await GetAsync<ResponseMain<QuotesHistoricalData>>(
                ExchangeApiUrls.QuotesHistorical(id.ToString(), string.Empty, timeStart, timeEnd, null, string.Empty,
                    new[] { string.Empty })).ConfigureAwait(false)
            : await GetAsync<ResponseMain<QuotesHistoricalData>>(
                ExchangeApiUrls.QuotesHistorical(string.Empty, idOrSlug, timeStart, timeEnd, null, string.Empty,
                    new[] { string.Empty })).ConfigureAwait(false);
    }

    public async Task<ResponseMain<Dictionary<string, QuotesLatestData>>> GetQuotesLatest(string slug, string[] convert)
    {
        return await GetAsync<ResponseMain<Dictionary<string, QuotesLatestData>>>(
            ExchangeApiUrls.QuotesLatest(null, slug, convert)).ConfigureAwait(false);
    }

    public async Task<ResponseMain<Dictionary<string, QuotesLatestData>>> GetQuotesLatest(long id, string[] convert)
    {
        return await GetAsync<ResponseMain<Dictionary<string, QuotesLatestData>>>(
            ExchangeApiUrls.QuotesLatest(id, string.Empty, convert)).ConfigureAwait(false);
    }
}