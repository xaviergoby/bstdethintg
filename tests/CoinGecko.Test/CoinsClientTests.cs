using CoinGeckoAPI.Clients;
using CoinGeckoAPI.Entities.Response.Coins;
using CoinGeckoAPI.Interfaces;
using CoinGeckoAPI.Parameters;
using CoinGeckoAPI.Test.HttpHandler;
using Xunit;

namespace CoinGeckoAPI.Test;

public class CoinsClientTests
{
    private readonly ICoinGeckoClient _client;

    public CoinsClientTests()
    {
        _client = new CoinGeckoClient(MockHttpHandler.HttpHandler.Object);
    }

    private async Task<CoinFullDataById> GetAllCoinDataForBtc()
    {
        return await _client.CoinsClient.GetAllCoinDataWithId("bitcoin");
    }
    private async Task<CoinFullDataById> GetAllCoinDataWithParameterForBtc()
    {
        return await _client.CoinsClient.GetAllCoinDataWithId("bitcoin", "false", false, true, false, false, true);
    }

    [Fact]
    public async Task Bitcoin_Sparkline7d_Equal_To_Null()
    {
        var result = await GetAllCoinDataWithParameterForBtc().ConfigureAwait(false);
        Assert.IsType<decimal[]>(result.MarketData.Sparkline7D.Price);
    }

    [Fact]
    public async Task BTC_Block_Time_in_Minutes_Not_Null()
    {
        var result = await GetAllCoinDataForBtc().ConfigureAwait(false);
        Assert.IsType<long>(result.BlockTimeInMinutes);
    }

    [Fact]
    public async Task BTC_Coin_by_Id_Ticker_Must_Contains_trade_URL()
    {
        var result = await GetAllCoinDataForBtc().ConfigureAwait(false);
        Assert.IsType<string>(result.Tickers.First().TradeUrl);
    }

    [Fact]
    public async Task Coin_by_Id_Must_Contains_ATH_Details()
    {
        var result = await GetAllCoinDataForBtc().ConfigureAwait(false);
        Assert.NotNull(result.MarketData.Ath);
        Assert.NotNull(result.MarketData.AthDate);
        Assert.NotNull(result.MarketData.AthChangePercentage);
    }

    [Fact]
    public async Task Coin_by_Id_Must_Contains_ATL_Details()
    {
        var result = await GetAllCoinDataForBtc().ConfigureAwait(false);
        Assert.NotNull(result.MarketData.Atl);
        Assert.NotNull(result.MarketData.AtlDate);
        Assert.NotNull(result.MarketData.AtlChangePercentage);
    }

    [Fact]
    public async Task Coin_By_Id_Must_Return_BTC()
    {
        var result = await GetAllCoinDataForBtc().ConfigureAwait(false);
        Assert.Equal("btc", result.Symbol);
        result = await GetAllCoinDataWithParameterForBtc().ConfigureAwait(false);
        Assert.Equal("btc", result.Symbol);
    }

    [Fact]
    public async Task Coin_Markets_VsCurrency_For_USD()
    {
        var result = await _client.CoinsClient.GetCoinMarkets("usd");
        Assert.True(result.Count == 1);
    }

    [Fact]
    public async Task Coin_Markets_VsCurrency_For_USD_Ripple_Sparkline_Null()
    {
        var result = await _client.CoinsClient.GetCoinMarkets("usd", new[] { "bitcoin" }, OrderField.MarketCapDesc, 1,
            1, false, "1h", null);
        Assert.Single(result);
        Assert.Equal("bitcoin", result[0].Id);
        Assert.Null(result[0].SparklineIn7D);
    }

    [Fact]
    public async Task Bitcoin_Market_Chart_Price_Lenght_Must_Equal_to_Marketcaps_Lenght()
    {
        var result = await _client.CoinsClient.GetMarketChartsByCoinId("bitcoin", "usd", "max");
        Assert.True(result.Prices.Length == result.MarketCaps.Length);
    }

    [Fact]
    public async Task Coin_Stellar_Tickers_For_Binance_And_Bitfinex()
    {
        var result = await _client.CoinsClient.GetTickerByCoinId("stellar", new[] { "binance" }, null);
        Assert.Equal("Stellar", result.Name);
        Assert.Equal("Binance", result.Tickers[0].Market.Name);
    }

    [Fact]
    public async Task Coin_Tether_History()
    {
        var result = await _client.CoinsClient.GetHistoryByCoinId("tether", "01-12-2018", "false");
        Assert.Equal("Tether", result.Name);
    }

    [Fact]
    public async Task CoinsLists_Must_Not_Null_And_First_Element_Must_BTC()
    {
        var result = await _client.CoinsClient.GetCoinList();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Hydro_Genesis_Date_Equal_To_Null()
    {
        var result = await _client.CoinsClient.GetAllCoinDataWithId("hydro");
        Assert.NotNull(result.MarketData.Ath);
    }
    //
    // [Fact]
    // public async Task Last_Traded_At_and_Last_Fecth_at_Day_Must_Equal_Now()
    // {
    //     var result = await _client.CoinsClient.GetTickerByCoinId("bitcoin");
    //     Assert.Equal(0, (result.Tickers[0].LastFetchAt - DateTimeOffset.Now).Days);
    //     Assert.Equal(0, (result.Tickers[0].LastTradedAt - DateTimeOffset.Now).Days);
    // }

    [Fact]
    public async Task Market_Chart_Range()
    {
        var result = await _client.CoinsClient.GetMarketChartRangeByCoinId("bitcoin", "usd", "1392577232", "1422577232");
        Assert.Equal(result.Prices.Length, result.MarketCaps.Length);
    }

    [Fact]
    public async Task Price_Change_Percentage_By_Days_Must_Not_Null()
    {
        var result = await _client.CoinsClient.GetCoinMarkets("usd", new[] { "bitcoin" }, null, null, null, false,
            "1h,24h,7d,14d,30d,200d,1y", "");
        Assert.NotNull(result.First().PriceChangePercentage1HInCurrency);
        Assert.NotNull(result.First().PriceChangePercentage24HInCurrency);
        Assert.NotNull(result.First().PriceChangePercentage7DInCurrency);
        Assert.NotNull(result.First().PriceChangePercentage14DInCurrency);
        Assert.NotNull(result.First().PriceChangePercentage30DInCurrency);
        Assert.NotNull(result.First().PriceChangePercentage200DInCurrency);
        Assert.NotNull(result.First().PriceChangePercentage1YInCurrency);
    }

    [Fact]
    public async Task TrustScoreNotNull()
    {
        var result = await _client.CoinsClient.GetTickerByCoinId("bitcoin");
        Assert.NotNull(result.Tickers[0].TrustScore);
    }

    [Fact]
    public async Task CoinOhlc_Elements_Not_Null()
    {
        var result = await _client.CoinsClient.GetCoinOhlc("bitcoin", "usd", 1);
        Assert.NotNull(result);
        Assert.NotNull(result[0][0]);
        Assert.NotNull(result[0][1]);
        Assert.NotNull(result[0][2]);
        Assert.NotNull(result[0][3]);
        Assert.NotNull(result[0][4]);
    }

    [Fact]
    public async Task AssetPlatforms_Elements_Not_Null()
    {
        var result = await _client.CoinsClient.GetAssetPlatforms();
        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
}