using CoinGeckoAPI.Entities.Response.Search;

namespace CoinGeckoAPI.Interfaces;

public interface ISearchClient
{
    /// <summary>
    /// Top-7 trending coins on CoinGecko as searched by users in the last 24 hours (Ordered by most popular first)
    /// </summary>
    /// <returns></returns>
    Task<TrendingList> GetSearchTrending();
}
