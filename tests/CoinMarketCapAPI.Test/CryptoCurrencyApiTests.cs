using CoinMaeketCapAPI.Test.HttpHandler;

namespace CoinMarketCapAPI.Test;

public class CryptoCurrencyApiTests
{
    private readonly CoinMarketCapClient _coinMarketCapClient;

    public CryptoCurrencyApiTests()
    {
        // Info on Sandbox API key: https://coinmarketcap.com/api/documentation/v1/
        _coinMarketCapClient = new CoinMarketCapClient(MockHttpHandler.HttpHandler.Object, ApiEnvironment.Sandbox, "b54bcf4d-1bca-4e8e-9a24-22ff2c3d462c");
    }

    // Gives different output in the Sandbox
    //[Fact]
    //public async Task GetMetaDataBySymbolShoudHaveData()
    //{
    //    var result = await _coinMarketCapClient.CryptoCurrencyClient.GetMetaData(new string[] { Currency.Btc, Currency.Xrp });
    //    Assert.NotNull(result);
    //    Assert.NotNull(result.Data);
    //    //Assert.Equal("BTC", result.Data.Values.First().Symbol);
    //    //Assert.Equal("XRP", result.Data.Values.Last().Symbol);
    //}

    [Fact]
    public async Task GetMetaDataByIdShouldHaveData()
    {
        var result = await _coinMarketCapClient.CryptoCurrencyClient.GetMetaData(new long[] { 1L });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetIdMapUsingAllParametersShouldReturnData()
    {
        // The Sandbox returns random data, not honoring the parameters.
        var result = await _coinMarketCapClient.CryptoCurrencyClient.GetIdMap(ListingStatus.Active, 1, 1, SortField.Id, new[] { "" });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetIdMapUsingLimitShouldReturnData()
    {
        // The Sandbox returns random data, not honoring the parameters.
        var result = await _coinMarketCapClient.CryptoCurrencyClient.GetIdMap(1, SortField.Id);
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    // This call gives different results than documented. Instead of an array, a dictionary is returned.
    //[Fact]
    //public async Task GetIdMapUsingSymbolShouldReturnData()
    //{
    //    var result = await _coinMarketCapClient.CryptoCurrencyClient.GetIdMap(new[] { "BTC" });
    //    Assert.NotNull(result);
    //    Assert.NotNull(result.Data);
    //}

    [Fact]
    public async Task GetListingLatestShouldReturnData()
    {
        var result = await _coinMarketCapClient.CryptoCurrencyClient.GetListingLatest(1, 2,
            new[] { "USD" }, SortField.MarketCap, "", CryptoCurrencyType.All);
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetMarketPairLatestBySymbolShouldReturnData()
    {
        var result = await _coinMarketCapClient.CryptoCurrencyClient.GetMarketPairLatest(Currency.Btc, 1, 1, new[] { Currency.Usd, Currency.Eur });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetMarketPairLatestByIdShouldReturnData()
    {
        var result = await _coinMarketCapClient.CryptoCurrencyClient.GetMarketPairLatest(1L, 1, 1, new[] { Currency.Usd, Currency.Eur });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetOhlvcHistoricalBySymbolShouldReturnData()
    {
        var startDate = (DateTime.Now + new TimeSpan(-18, 0, 0, 0)).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
        var finishDate = (DateTime.Now + new TimeSpan(-15, 0, 0, 0)).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
        var result = await _coinMarketCapClient.CryptoCurrencyClient.GetOhlvcHistorical(Currency.Btc, Interval.Daily,
            startDate, finishDate, 10, Interval.Daily, new string[] { Currency.Usd });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetOhlvcHistoricalByIdShouldReturnData()
    {
        var startDate = (DateTime.Now + new TimeSpan(-18, 0, 0, 0)).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
        var finishDate = (DateTime.Now + new TimeSpan(-15, 0, 0, 0)).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
        var result = await _coinMarketCapClient.CryptoCurrencyClient.GetOhlvcHistorical(1L, Interval.Daily,
            startDate, finishDate, 10, Interval.Daily, new string[] { Currency.Usd });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task OhlcvLatestBySymbolShouldReturnData()
    {
        var result = await _coinMarketCapClient.CryptoCurrencyClient.GetOhlcvLatest(new[] { Currency.Btc }, new[] { Currency.Usd });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task OhlcvLatestByIdShouldReturnData()
    {
        var result = await _coinMarketCapClient.CryptoCurrencyClient.GetOhlcvLatest(new[] { 1L }, new[] { Currency.Usd });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task QuotesHistoricalBySymbolShouldReturnData()
    {
        var startDate = (DateTime.Now + new TimeSpan(-18, 0, 0, 0)).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
        var finishDate = (DateTime.Now + new TimeSpan(-15, 0, 0, 0)).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
        var result = await _coinMarketCapClient.CryptoCurrencyClient.GetQuotesHistorical("BTC",
            startDate, finishDate, 10, Interval.Daily, new[] { Currency.Usd });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task QuotesHistoricalByIdShouldReturnData()
    {
        var startDate = (DateTime.Now + new TimeSpan(-18, 0, 0, 0)).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
        var finishDate = (DateTime.Now + new TimeSpan(-15, 0, 0, 0)).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
        var result = await _coinMarketCapClient.CryptoCurrencyClient.GetQuotesHistorical(1L,
            startDate, finishDate, 10, Interval.Daily, new[] { Currency.Usd });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task QuotesLatestBySymbolShouldReturnData()
    {
        var result = await _coinMarketCapClient.CryptoCurrencyClient.GetQuotesLatest(new[] { Currency.Btc }, new[] { Currency.Usd });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task QuotesLatestByIdShouldReturnData2()
    {
        var result = await _coinMarketCapClient.CryptoCurrencyClient.GetQuotesLatest(new[] { 1L }, new[] { Currency.Usd });
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }
}