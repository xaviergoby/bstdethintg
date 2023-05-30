using CoinGeckoAPI.ApiEndPoints;
using CoinGeckoAPI.Entities.Response.Global;
using CoinGeckoAPI.Interfaces;
using System.Text.Json;

namespace CoinGeckoAPI.Clients;

public class GlobalClient : BaseApiClient, IGlobalClient
{
    public GlobalClient(HttpClient httpClient, JsonSerializerOptions serializerOptions) : base(httpClient, serializerOptions)
    {
    }

    public GlobalClient(HttpClient httpClient, JsonSerializerOptions serializerOptions, string apiKey) : base(httpClient, serializerOptions, apiKey)
    {
    }

    public async Task<Global> GetGlobal()
    {
        return await GetAsync<Global>(AppendQueryString(GlobalApiEndPoints.Global)).ConfigureAwait(false);
    }

    public async Task<GlobalDeFi> GetGlobalDeFi()
    {
        return await GetAsync<GlobalDeFi>(AppendQueryString(GlobalApiEndPoints.DecentralizedFinanceDeFi)).ConfigureAwait(false);
    }
}