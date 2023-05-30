using CoinGeckoAPI.Entities.Response.Indexes;

namespace CoinGeckoAPI.Interfaces;

public interface IIndexesClient
{
    Task<IReadOnlyList<IndexData>> GetIndexes();
    Task<IReadOnlyList<IndexData>> GetIndexes(int? perPage, string page);
    //Task<IndexData> GetIndexById(string id);
    Task<IReadOnlyList<IndexList>> GetIndexList();
}