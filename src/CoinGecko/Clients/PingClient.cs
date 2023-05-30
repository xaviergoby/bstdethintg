using CoinGeckoAPI.Entities.Response;
using CoinGeckoAPI.Interfaces;
using System.Text.Json;

namespace CoinGeckoAPI.Clients;

public class PingClient : BaseApiClient, IPingClient
{
    public PingClient(HttpClient httpClient, JsonSerializerOptions serializerOptions) : base(httpClient, serializerOptions)
    {
    }

    public PingClient(HttpClient httpClient, JsonSerializerOptions serializerOptions, string apiKey) : base(httpClient, serializerOptions, apiKey)
    {
    }

    public async Task<Ping> GetPingAsync() => await GetAsync<Ping>(AppendQueryString("ping")).ConfigureAwait(false);
}