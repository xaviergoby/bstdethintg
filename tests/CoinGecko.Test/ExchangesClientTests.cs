using CoinGeckoAPI.Clients;
using CoinGeckoAPI.Interfaces;
using CoinGeckoAPI.Test.HttpHandler;
using Xunit;

namespace CoinGeckoAPI.Test;

public class ExchangesClientTests
{
    private readonly ICoinGeckoClient _client;

    public ExchangesClientTests()
    {
        _client = new CoinGeckoClient(MockHttpHandler.HttpHandler.Object);
    }

    [Fact]
    public async Task Exchanges_Count_Not_Equal_to_Zero()
    {
        var result = await _client.ExchangesClient.GetExchanges();
        Assert.IsType<int>(result.Count);
        Assert.NotEqual(0, result.Count);
    }

    [Fact]
    public async Task Exchanges_Images_Url_Count_Must_Equal_Total_Count()
    {
        var result = await _client.ExchangesClient.GetExchanges();
        var notNullImages = result.Where(x => x.Image != "").ToList();
        var notNullUrl = result.Where(x => x.Url != "").ToList();
        Assert.Equal(result.Count, notNullImages.Count);
        Assert.Equal(result.Count, notNullUrl.Count);
    }

    [Fact]
    public async Task Exchanges_For_Bitfinex()
    {
        var result = await _client.ExchangesClient.GetExchangesByExchangeId("bitfinex");
        Assert.Equal("Bitfinex", result.Name);
    }

    [Fact]
    public async Task Exchanges_Bitfinex_Tickers()
    {
        var result = await _client.ExchangesClient.GetTickerByExchangeId("bitfinex");
        Assert.Equal("Bitfinex", result.Name);
    }

    [Fact]
    public async Task Exchanges_Bitfinex_Tickers_For_Bitcoin_And_Ripple()
    {
        var result = await _client.ExchangesClient.GetTickerByExchangeId("bitfinex", new[] { "bitcoin", "ripple" }, null, "", "");
        Assert.Equal("Bitfinex", result.Name);
        var xrpTicker = result.Tickers.Where(x => x.Base == "XRP").FirstOrDefault();
        var btcTicker = result.Tickers.Where(x => x.Base == "BTC").FirstOrDefault();
        Assert.NotNull(xrpTicker);
        Assert.NotNull(btcTicker);
    }

    [Fact]
    public async Task Exchanges_List_Not_Null()
    {
        var result = await _client.ExchangesClient.GetExchangesList();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Exchanges_Uniswap_NonEmpty_Properties()
    {
        var result = await _client.ExchangesClient.GetExchangesByExchangeId("uniswap_v2");
        Assert.NotNull(result);
        Assert.False(result.Centralized);
        Assert.Equal("https://www.reddit.com/r/UniSwap/", result.RedditUrl);
        Assert.Equal("UniswapExchange", result.TwitterHandle);
    }
}