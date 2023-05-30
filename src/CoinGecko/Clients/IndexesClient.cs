using CoinGeckoAPI.ApiEndPoints;
using CoinGeckoAPI.Entities.Response.Indexes;
using CoinGeckoAPI.Interfaces;
using System.Text.Json;

namespace CoinGeckoAPI.Clients;

public class IndexesClient : BaseApiClient, IIndexesClient
{
    public IndexesClient(HttpClient httpClient, JsonSerializerOptions serializerOptions) : base(httpClient, serializerOptions)
    {
    }

    public IndexesClient(HttpClient httpClient, JsonSerializerOptions serializerOptions, string apiKey) : base(httpClient, serializerOptions, apiKey)
    {
    }

    public async Task<IReadOnlyList<IndexData>> GetIndexes()
    {
        return await GetIndexes(null, "").ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<IndexData>> GetIndexes(int? perPage, string page)
    {
        return await GetAsync<IReadOnlyList<IndexData>>(AppendQueryString(
            IndexesApiEndPointUrl.IndexesUrl, new Dictionary<string, object>
            {
                {"per_page",perPage},
                {"page",page}
            })).ConfigureAwait(false);
    }

    public async Task<IndexData> GetIndexById(string id)
    {
        return await GetAsync<IndexData>(AppendQueryString(
            IndexesApiEndPointUrl.IndexesWithId(id))).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<IndexList>> GetIndexList()
    {
        return await GetAsync<IReadOnlyList<IndexList>>(AppendQueryString(
            IndexesApiEndPointUrl.IndexesList)).ConfigureAwait(false);
    }
}