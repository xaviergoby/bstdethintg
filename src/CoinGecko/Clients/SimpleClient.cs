using CoinGeckoAPI.ApiEndPoints;
using CoinGeckoAPI.Entities.Response.Simple;
using CoinGeckoAPI.Interfaces;
using System.Text.Json;

namespace CoinGeckoAPI.Clients;

public class SimpleClient : BaseApiClient, ISimpleClient
{
    public SimpleClient(HttpClient httpClient, JsonSerializerOptions serializerOptions) : base(httpClient, serializerOptions)
    {
    }

    public SimpleClient(HttpClient httpClient, JsonSerializerOptions serializerOptions, string apiKey) : base(httpClient, serializerOptions, apiKey)
    {
    }

    public async Task<Price> GetSimplePrice(string[] ids, string[] vsCurrencies) =>
        await GetSimplePrice(ids, vsCurrencies, false, false, false, false).ConfigureAwait(false);

    public async Task<Price> GetSimplePrice(string[] ids, string[] vsCurrencies, bool includeMarketCap,
        bool include24HVol, bool include24HChange, bool includeLastUpdatedAt)
    {
        return await GetAsync<Price>(AppendQueryString(SimpleApiEndPoints.SimplePrice,
            new Dictionary<string, object>
            {
                {"ids", string.Join(",",ids)},
                {"vs_currencies",string.Join(",",vsCurrencies)},
                {"include_market_cap",includeMarketCap},
                {"include_24hr_vol",include24HVol},
                {"include_24hr_change",include24HChange},
                {"include_last_updated_at",includeLastUpdatedAt}
            })).ConfigureAwait(false);
    }

    public async Task<TokenPrice> GetTokenPrice(string id, string[] contractAddress, string[] vsCurrencies) =>
        await GetTokenPrice(id, contractAddress, vsCurrencies, false, false, false, false).ConfigureAwait(false);

    public async Task<TokenPrice> GetTokenPrice(string id, string[] contractAddress, string[] vsCurrencies, bool includeMarketCap,
        bool include24HVol, bool include24HChange, bool includeLastUpdatedAt) =>
        await GetAsync<TokenPrice>(AppendQueryString(SimpleApiEndPoints.TokenPrice(id),
            new Dictionary<string, object>
            {
                {"contract_addresses",string.Join(",",contractAddress)},
                {"vs_currencies",string.Join(",",vsCurrencies)},
                {"include_market_cap",includeMarketCap},
                {"include_24hr_vol",include24HVol},
                {"include_24hr_change",include24HChange},
                {"include_last_updated_at",includeLastUpdatedAt}
            })).ConfigureAwait(false);

    public async Task<SupportedCurrencies> GetSupportedVsCurrencies() => await GetAsync<SupportedCurrencies>(
            AppendQueryString(SimpleApiEndPoints.SimpleSupportedVsCurrencies)).ConfigureAwait(false);
}