using CoinGeckoAPI.Clients;
using CoinGeckoAPI.Interfaces;
using CoinGeckoAPI.Test.HttpHandler;
using Xunit;

namespace CoinGeckoAPI.Test;

public class GlobalClientTests
{
    private readonly ICoinGeckoClient _client;

    public GlobalClientTests()
    {
        _client = new CoinGeckoClient(MockHttpHandler.HttpHandler.Object);
    }

    [Fact]
    public async Task Global_Data_Must_Not_Null()
    {
        var result = await _client.GlobalClient.GetGlobal();
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task Global_DeFi_Data_Type_Must_Equal()
    {
        var result = await _client.GlobalClient.GetGlobalDeFi();
        Assert.IsType<decimal>(result.Data.DeFiMarketCap);
    }
}