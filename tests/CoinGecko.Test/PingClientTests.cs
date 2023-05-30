using CoinGeckoAPI.Clients;
using CoinGeckoAPI.Interfaces;
using CoinGeckoAPI.Test.HttpHandler;
using Xunit;

namespace CoinGeckoAPI.Test;

public class PingClientTests
{
    private readonly ICoinGeckoClient _client;

    public PingClientTests()
    {
        _client = new CoinGeckoClient(MockHttpHandler.HttpHandler.Object);
    }

    [Fact]
    public async Task Ping_Method_Must_Return_ToTheMoon()
    {
        var result = await _client.PingClient.GetPingAsync();
        Assert.Equal("(V3) To the Moon!", result.GeckoSays);
    }
}