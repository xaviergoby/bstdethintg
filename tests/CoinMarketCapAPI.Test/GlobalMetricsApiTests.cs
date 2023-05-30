using CoinMaeketCapAPI.Test.HttpHandler;

namespace CoinMarketCapAPI.Test;

public class GlobalMetricsApiTests
{
    private readonly CoinMarketCapClient _coinMarketCapClient;
    public GlobalMetricsApiTests()
    {
        // Info on Sandbox API key: https://coinmarketcap.com/api/documentation/v1/
        _coinMarketCapClient = new CoinMarketCapClient(MockHttpHandler.HttpHandler.Object, ApiEnvironment.Sandbox, "b54bcf4d-1bca-4e8e-9a24-22ff2c3d462c");
    }

    [Fact]
    public async Task GlobalMetricsHistorical()
    {
        var startDate = (DateTime.Now + new TimeSpan(-30, 0, 0, 0)).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
        var finishDate = (DateTime.Now + new TimeSpan(-15, 0, 0, 0)).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
        var result = await _coinMarketCapClient.GlobalMetricClient.GetGlobalMetricsHistorical(
            startDate, finishDate, 10, Interval.Daily, new[] { Currency.Usd });
        Assert.Equal("USD", result.Data.Quotes.First().Quote.Keys.First());
    }
    [Fact]
    public async Task GlobalMetricsHistorical_Default_Values()
    {
        var startDate = (DateTime.Now + new TimeSpan(-16, 0, 0, 0)).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
        var finishDate = (DateTime.Now + new TimeSpan(-15, 0, 0, 0)).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
        var result = await _coinMarketCapClient.GlobalMetricClient.GetGlobalMetricsHistorical(
            startDate, finishDate, 10, Interval.Daily, new[] { Currency.Usd });
        Assert.True(result.Data.Quotes[0].Quote.ContainsKey("USD"));
    }
    [Fact]
    public async Task GlobalMetricsLatest()
    {
        var result = await _coinMarketCapClient.GlobalMetricClient.GetGlobalMetricsLatest(new[] { Currency.Usd });

        Assert.NotNull(result.Data);
        Assert.Equal("USD", result.Data.Quote.First().Key);
    }
    [Fact]
    public async Task GlobalMetricsLatest_Default_Values()
    {
        var result = await _coinMarketCapClient.GlobalMetricClient.GetGlobalMetricsLatest();

        Assert.NotNull(result.Data);
        Assert.Equal("USD", result.Data.Quote.First().Key);
    }
}
