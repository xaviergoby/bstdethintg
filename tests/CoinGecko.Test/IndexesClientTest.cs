using CoinGeckoAPI.Clients;
using CoinGeckoAPI.Interfaces;
using CoinGeckoAPI.Test.HttpHandler;
using Xunit;

namespace CoinGeckoAPI.Test;

public class IndexesClientTest
{
    private readonly ICoinGeckoClient _client;
    public IndexesClientTest()
    {
        _client = new CoinGeckoClient(MockHttpHandler.HttpHandler.Object);
    }

    [Fact]
    public async Task Indexes_Count_Not_Equal_Zero()
    {
        var result = await _client.IndexesClient.GetIndexes();
        Assert.True(result.Count > 0, "Result GTE 0");
    }

    [Fact]
    public async Task Indexes_List_Count_Not_Equal_Zero()
    {
        var result = await _client.IndexesClient.GetIndexList();
        Assert.True(result.Count > 0, "Result GTE 0");
    }
}