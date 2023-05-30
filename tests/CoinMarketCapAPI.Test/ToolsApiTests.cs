using CoinMaeketCapAPI.Test.HttpHandler;

namespace CoinMarketCapAPI.Test;

public class ToolsApiTests
{
    private readonly CoinMarketCapClient _coinMarketCapClient;
    public ToolsApiTests()
    {
        // Info on Sandbox API key: https://coinmarketcap.com/api/documentation/v1/
        _coinMarketCapClient = new CoinMarketCapClient(MockHttpHandler.HttpHandler.Object, ApiEnvironment.Sandbox, "b54bcf4d-1bca-4e8e-9a24-22ff2c3d462c");
    }

    [Fact]
    public async Task BitcoinToUsdPriceConversion()
    {
        var startDate = (DateTime.Now + new TimeSpan(-16, 0, 0, 0)).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'");
        var result = await _coinMarketCapClient.ToolsClient.GetPriceConversion(1, "", Currency.Btc, startDate, new[] { Currency.Usd });
        Assert.NotNull(result.Data);
    }
}
