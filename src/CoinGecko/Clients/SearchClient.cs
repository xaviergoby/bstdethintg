using CoinGeckoAPI.ApiEndPoints;
using CoinGeckoAPI.Entities.Response.Search;
using CoinGeckoAPI.Interfaces;
using System.Text.Json;

namespace CoinGeckoAPI.Clients;

public class SearchClient : BaseApiClient, ISearchClient
{
    public SearchClient(HttpClient httpClient, JsonSerializerOptions jsonSerializerSetting) : base(httpClient, jsonSerializerSetting)
    {
    }

    public SearchClient(HttpClient httpClient, JsonSerializerOptions jsonSerializerSetting, string apiKey) : base(httpClient, jsonSerializerSetting, apiKey)
    {
    }

    public async Task<TrendingList> GetSearchTrending() => await GetAsync<TrendingList>(
            AppendQueryString(SearchApiEndpoints.SearchTrending))
        .ConfigureAwait(false);
}