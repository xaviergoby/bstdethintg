using CoinGeckoAPI.Clients;
using CoinGeckoAPI.Interfaces;
using CoinGeckoAPI.Test.HttpHandler;
using Xunit;

namespace CoinGeckoAPI.Test;

public class SearchClientTests
{
    private readonly ICoinGeckoClient _client;

    public SearchClientTests()
    {
        _client = new CoinGeckoClient(MockHttpHandler.HttpHandler.Object);
    }

    [Fact]
    public async Task SearchTrending_TrendingItems_Fields_Not_Null()
    {
        var result = (await _client.SearchClient.GetSearchTrending()).TrendingItems[0].TrendingItem;
        Assert.NotNull(result.Id);
    }
}
