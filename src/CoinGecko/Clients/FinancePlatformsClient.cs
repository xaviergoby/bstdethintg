using CoinGeckoAPI.ApiEndPoints;
using CoinGeckoAPI.Entities.Response.Finance;
using CoinGeckoAPI.Interfaces;
using System.Text.Json;

namespace CoinGeckoAPI.Clients;

public class FinancePlatformsClient : BaseApiClient, IFinancePlatformsClient
{
    public FinancePlatformsClient(HttpClient httpClient, JsonSerializerOptions serializerOptions) : base(httpClient, serializerOptions)
    {
    }

    public FinancePlatformsClient(HttpClient httpClient, JsonSerializerOptions serializerOptions, string apiKey) : base(httpClient, serializerOptions, apiKey)
    {
    }

    public async Task<IReadOnlyList<FinancePlatforms>> GetFinancePlatforms()
    {
        return await GetFinancePlatforms(50, "100").ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<FinancePlatforms>> GetFinancePlatforms(int perPage, string page)
    {
        return await GetAsync<IReadOnlyList<FinancePlatforms>>(AppendQueryString(
            FinancePlatformsApiEndPoints.FinancePlatform, new Dictionary<string, object>
            {
                {"per_page",perPage},
                {"page",page}
            }
        )).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<FinanceProducts>> GetFinanceProducts()
    {
        return await GetFinanceProducts(50, "100", "", "").ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<FinanceProducts>> GetFinanceProducts(int perPage, string page, string startAt, string endAt)
    {
        return await GetAsync<IReadOnlyList<FinanceProducts>>(AppendQueryString(
                FinancePlatformsApiEndPoints.FinanceProducts, new Dictionary<string, object>
                {
                    {"per_page",perPage},
                    {"page",page},
                    {"startAt",startAt},
                    {"endAt",endAt}
                }))
            .ConfigureAwait(false);
    }
}