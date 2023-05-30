using CoinGeckoAPI.ApiEndPoints;
using CoinGeckoAPI.Entities.Response.Derivatives;
using CoinGeckoAPI.Interfaces;
using System.Text.Json;

namespace CoinGeckoAPI.Clients;

public class DerivativesClient : BaseApiClient, IDerivativesClient
{
    public DerivativesClient(HttpClient httpClient, JsonSerializerOptions serializerOptions) : base(httpClient, serializerOptions)
    {
    }

    public DerivativesClient(HttpClient httpClient, JsonSerializerOptions serializerOptions, string apiKey) : base(httpClient, serializerOptions, apiKey)
    {
    }


    public async Task<IReadOnlyList<Derivatives>> GetDerivatives()
    {
        return await GetDerivatives("").ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Derivatives>> GetDerivatives(string includeTicker)
    {
        return await GetAsync<IReadOnlyList<Derivatives>>(AppendQueryString(
            DerivativesApiEndPoints.DerivativesUrl, new Dictionary<string, object>
            {
                {"include_ticker",includeTicker}
            })).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<DerivativesExchanges>> GetDerivativesExchanges()
    {
        return await GetDerivativesExchanges("", null, null).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<DerivativesExchanges>> GetDerivativesExchanges(string order, int? perPage, int? page)
    {
        return await GetAsync<IReadOnlyList<DerivativesExchanges>>(AppendQueryString(
            DerivativesApiEndPoints.DerivativesExchanges, new Dictionary<string, object>
            {
                {"order",order},
                {"per_page",perPage},
                {"page",page}
            })).ConfigureAwait(false);
    }

    public async Task<DerivativesExchanges> GetDerivativesExchangesById(string id)
    {
        return await GetDerivativesExchangesById(id, "").ConfigureAwait(false);
    }

    public async Task<DerivativesExchanges> GetDerivativesExchangesById(string id, string includeTickers)
    {
        return await GetAsync<DerivativesExchanges>(AppendQueryString(
            DerivativesApiEndPoints.DerivativesExchangeById(id), new Dictionary<string, object>
            {
                {"include_tickers", includeTickers}
            })).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<DerivativesExchangesList>> GetDerivativesExchangesList()
    {
        return await GetAsync<IReadOnlyList<DerivativesExchangesList>>(
                AppendQueryString(DerivativesApiEndPoints.DerivativesExchangesList))
            .ConfigureAwait(false);
    }
}