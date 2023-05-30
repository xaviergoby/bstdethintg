using CoinGeckoAPI.Clients;
using CoinGeckoAPI.Interfaces;
using CoinGeckoAPI.Test.HttpHandler;
using Xunit;

namespace CoinGeckoAPI.Test;

public class ExchangeRatesClientTests
{
    private readonly ICoinGeckoClient _client;

    public ExchangeRatesClientTests()
    {
        _client = new CoinGeckoClient(MockHttpHandler.HttpHandler.Object);
    }

    [Fact]
    public async Task Exchange_Rates_Cointains_Eos()
    {
        var result = await _client.ExchangeRatesClient.GetExchangeRates();
        Assert.True(result.Rates.ContainsKey("eos"));
    }

}