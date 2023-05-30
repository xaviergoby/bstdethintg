using CoinGeckoAPI.ApiEndPoints;
using CoinGeckoAPI.Entities.Response.ExchangeRates;
using CoinGeckoAPI.Interfaces;
using System.Text.Json;

namespace CoinGeckoAPI.Clients;

public class ExchangeRatesClient : BaseApiClient, IExchangeRatesClient
{
    public ExchangeRatesClient(HttpClient httpClient, JsonSerializerOptions serializerOptions) : base(httpClient, serializerOptions)
    {
    }

    public ExchangeRatesClient(HttpClient httpClient, JsonSerializerOptions serializerOptions, string apiKey) : base(httpClient, serializerOptions, apiKey)
    {
    }

    public async Task<ExchangeRates> GetExchangeRates()
    {
        return await GetAsync<ExchangeRates>(
            AppendQueryString(ExchangeRatesApiEndPoints.ExchangeRate)).ConfigureAwait(false);
    }
}