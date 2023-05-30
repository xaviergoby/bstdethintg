using CoinGeckoAPI.ApiEndPoints;
using CoinGeckoAPI.Entities.Response.Coins;
using CoinGeckoAPI.Interfaces;
using CoinGeckoAPI.Parameters;
using System.Text.Json;

namespace CoinGeckoAPI.Clients;

public class CoinsClient : BaseApiClient, ICoinsClient
{
    public CoinsClient(HttpClient httpClient, JsonSerializerOptions serializerOptions) : base(httpClient, serializerOptions)
    {
    }

    public CoinsClient(HttpClient httpClient, JsonSerializerOptions serializerOptions, string apiKey) : base(httpClient, serializerOptions, apiKey)
    {
    }

    public async Task<IReadOnlyList<CoinFullData>> GetAllCoinsData() =>
        await GetAllCoinsData(OrderField.GeckoDesc, null, null, "", null).ConfigureAwait(false);

    public async Task<IReadOnlyList<CoinFullData>> GetAllCoinsData(string order, int? perPage, int? page, string localization, bool? sparkline) =>
        await GetAsync<IReadOnlyList<CoinFullData>>(AppendQueryString(
            CoinsApiEndPoints.Coins, new Dictionary<string, object>
            {
                {"order", order},
                {"per_page", perPage},
                {"page",page},
                {"localization",localization},
                {"sparkline",sparkline}
            })).ConfigureAwait(false);

    public async Task<IReadOnlyList<CoinList>> GetCoinList() =>
        await GetAsync<IReadOnlyList<CoinList>>(AppendQueryString(CoinsApiEndPoints.CoinList)).ConfigureAwait(false);

    public async Task<IReadOnlyList<CoinList>> GetCoinList(bool includePlatform) =>
        await GetAsync<IReadOnlyList<CoinList>>(AppendQueryString(CoinsApiEndPoints.CoinList,
            new Dictionary<string, object>
            {
                {
                    "include_platform",includePlatform.ToString()
                }
            })).ConfigureAwait(false);

    public async Task<List<CoinMarkets>> GetCoinMarkets(string vsCurrency) =>
        await GetCoinMarkets(vsCurrency, Array.Empty<string>(), null, null, null, false, null, null).ConfigureAwait(false);

    public async Task<List<CoinMarkets>> GetCoinMarkets(string vsCurrency, string[] ids, string order, int? perPage,
        int? page, bool sparkline, string priceChangePercentage) =>
        await GetCoinMarkets(vsCurrency, ids, order, perPage, page, sparkline, priceChangePercentage, null).ConfigureAwait(false);

    public async Task<List<CoinMarkets>> GetCoinMarkets(string vsCurrency, string[] ids, string order, int? perPage,
        int? page, bool sparkline, string priceChangePercentage, string category) =>
        await GetAsync<List<CoinMarkets>>(AppendQueryString(CoinsApiEndPoints.CoinMarkets,
            new Dictionary<string, object>
            {
                {"vs_currency", vsCurrency},
                {"ids", string.Join(",", ids)},
                {"order",order},
                {"per_page", perPage},
                {"page", page},
                {"sparkline", sparkline},
                {"price_change_percentage", priceChangePercentage},
                {"category",category}
            })).ConfigureAwait(false);

    public async Task<CoinFullDataById> GetAllCoinDataWithId(string id) =>
        await GetAllCoinDataWithId(id, "true", true, true, true, true, false).ConfigureAwait(false);

    public async Task<CoinFullDataById> GetAllCoinDataWithId(string id, string localization, bool tickers,
        bool marketData, bool communityData, bool developerData, bool sparkline) =>
        await GetAsync<CoinFullDataById>(AppendQueryString(
            CoinsApiEndPoints.AllDataByCoinId(id), new Dictionary<string, object>
            {
                {"localization", localization},
                {"tickers", tickers},
                {"market_data", marketData},
                {"community_data", communityData},
                {"developer_data", developerData},
                {"sparkline", sparkline}
            })).ConfigureAwait(false);

    public async Task<TickerById> GetTickerByCoinId(string id) =>
        await GetTickerByCoinId(id, new[] { "" }, null).ConfigureAwait(false);

    public async Task<TickerById> GetTickerByCoinId(string id, int? page) =>
        await GetTickerByCoinId(id, new[] { "" }, page).ConfigureAwait(false);

    public async Task<TickerById> GetTickerByCoinId(string id, string[] exchangeIds, int? page) =>
        await GetTickerByCoinId(id, exchangeIds, page, "", OrderField.TrustScoreDesc, false).ConfigureAwait(false);

    public async Task<TickerById> GetTickerByCoinId(string id, string[] exchangeIds, int? page,
        string includeExchangeLogo, string order, bool depth) =>
        await GetAsync<TickerById>(AppendQueryString(
            CoinsApiEndPoints.TickerByCoinId(id), new Dictionary<string, object>
            {
                {"page", page},
                {"exchange_ids",string.Join(",",exchangeIds)},
                {"include_exchange_logo",includeExchangeLogo},
                {"order",order},
                {"depth",depth.ToString()}
            })).ConfigureAwait(false);

    public async Task<CoinFullData> GetHistoryByCoinId(string id, string date, string localization) =>
        await GetAsync<CoinFullData>(AppendQueryString(
            CoinsApiEndPoints.HistoryByCoinId(id), new Dictionary<string, object>
            {
                {"date",date},
                {"localization",localization}
            })).ConfigureAwait(false);

    public async Task<MarketChartById> GetMarketChartsByCoinId(string id, string vsCurrency, string days) =>
        await GetMarketChartsByCoinId(id, vsCurrency, days, "").ConfigureAwait(false);

    public async Task<MarketChartById> GetMarketChartsByCoinId(string id, string vsCurrency, string days, string interval) =>
        await GetAsync<MarketChartById>(AppendQueryString(
            CoinsApiEndPoints.MarketChartByCoinId(id),
            new Dictionary<string, object>
            {
                {"vs_currency", string.Join(",",vsCurrency)},
                {"days", days},
                {"interval",interval}
            })).ConfigureAwait(false);

    public async Task<MarketChartById> GetMarketChartRangeByCoinId(string id, string vsCurrency, string @from, string to) =>
        await GetAsync<MarketChartById>(AppendQueryString(
            CoinsApiEndPoints.MarketChartRangeByCoinId(id), new Dictionary<string, object>
            {
                {"vs_currency", string.Join(",", vsCurrency)},
                {"from",from},
                {"to",to}
            })).ConfigureAwait(false);

    public async Task<IReadOnlyList<IReadOnlyList<object>>> GetCoinOhlc(string id, string vsCurrency, int days) =>
        await GetAsync<IReadOnlyList<IReadOnlyList<object>>>(AppendQueryString(
            CoinsApiEndPoints.CoinOhlc(id), new Dictionary<string, object>
            {
                {"vs_currency", vsCurrency},
                {"days", days}
            }));

    public async Task<IReadOnlyList<AssetPlatform>> GetAssetPlatforms() =>
        await GetAsync<IReadOnlyList<AssetPlatform>>(AppendQueryString(CoinsApiEndPoints.AssetPlatforms)).ConfigureAwait(false);
}