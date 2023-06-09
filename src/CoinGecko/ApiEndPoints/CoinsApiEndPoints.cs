namespace CoinGeckoAPI.ApiEndPoints;

public static class CoinsApiEndPoints
{
    public static readonly string Coins = "coins";
    public static readonly string CoinList = "coins/list";
    public static readonly string CoinMarkets = "coins/markets";
    public static readonly string AssetPlatforms = "asset_platforms";

    public static string AllDataByCoinId(string id) => BaseApiEndPointUrl.AddCoinsIdUrl(id);
    public static string TickerByCoinId(string id) => BaseApiEndPointUrl.AddCoinsIdUrl(id) + "/tickers";
    public static string HistoryByCoinId(string id) => BaseApiEndPointUrl.AddCoinsIdUrl(id) + "/history";
    public static string MarketChartByCoinId(string id) => BaseApiEndPointUrl.AddCoinsIdUrl(id) + "/market_chart";
    public static string MarketChartRangeByCoinId(string id) => BaseApiEndPointUrl.AddCoinsIdUrl(id) + "/market_chart/range";
    public static string CoinOhlc(string id) => BaseApiEndPointUrl.AddCoinsIdUrl(id) + "/ohlc";
}