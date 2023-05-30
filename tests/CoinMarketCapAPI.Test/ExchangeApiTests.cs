using CoinMaeketCapAPI.Test.HttpHandler;

namespace CoinMarketCapAPI.Test;

public class ExchangeApiTests
{
    private readonly CoinMarketCapClient _coinMarketCapClient;
    public ExchangeApiTests()
    {
        // Info on Sandbox API key: https://coinmarketcap.com/api/documentation/v1/
        _coinMarketCapClient = new CoinMarketCapClient(MockHttpHandler.HttpHandler.Object, ApiEnvironment.Sandbox, "b54bcf4d-1bca-4e8e-9a24-22ff2c3d462c");
    }

    [Fact]
    public async Task ExchangeInfoMustReturnData()
    {
        var result = await _coinMarketCapClient.ExchangeClient.GetInfo("", "binance");
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    // Sandbox gives different results
    //[Fact]
    //public async Task MapMustReturnBinance()
    //{
    //    var result = await _coinMarketCapClient.ExchangeClient.GetMap("", "binance", 1, 100);
    //    Assert.NotNull(result);
    //    Assert.NotNull(result.Data);
    //}

    [Fact]
    public async Task ListingLatestShouldReturnData()
    {
        var result = await _coinMarketCapClient.ExchangeClient.GetListingLatest(1, 100, SortField.Volume24H, SortDirection.Desc, MarketType.Fees, new[] { "" });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task MarketPairsLatest()
    {
        var result = await _coinMarketCapClient.ExchangeClient.GetMarketPairsLatest("", "binance", 1, 1, new[] { Currency.Usd });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task QuotesHistoricalShouldReturnData()
    {
        var startDate = (DateTime.Now + new TimeSpan(-16, 0, 0, 0)).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
        var finishDate = (DateTime.Now + new TimeSpan(-15, 0, 0, 0)).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
        var result = await _coinMarketCapClient.ExchangeClient.GetQuotesHistorical("", "binance",
            startDate, finishDate, 1, "", new[] { Currency.Usd });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task QuotesLatestBySlugShouldReturnDate()
    {
        var result = await _coinMarketCapClient.ExchangeClient.GetQuotesLatest("binance", new[] { Currency.Usd });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task QuotesLatestByIdShouldReturnDate()
    {
        var result = await _coinMarketCapClient.ExchangeClient.GetQuotesLatest(1L, new[] { Currency.Usd });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

}
