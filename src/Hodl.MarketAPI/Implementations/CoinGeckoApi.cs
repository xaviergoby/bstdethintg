using CoinGeckoAPI.Clients;
using CoinGeckoAPI.Entities.Response.Coins;
using Hodl.Framework;
using Hodl.MarketAPI.Models;
using Hodl.Utils;

namespace Hodl.MarketAPI.Implementations;

public class CoinGeckoApi : ExternalApi, ICryptoMarketApi
{
    private const int CACHE_TIME_IN_HOURS = 24;
    private const int COINGECKO_MARKETS_COUNT_PER_FETCH = 100;

    private readonly CoinGeckoOptions _coinGeckoOptions;
    private readonly CoinGeckoClient _coinGeckoClient;

    private static IReadOnlyList<CoinList> _coins;
    private static DateTimeOffset _lastCoinsUpdateTime = DateTimeOffset.MinValue;
    private static IReadOnlyList<AssetPlatform> _platforms;
    private static DateTimeOffset _lastPlatformsUpdateTime = DateTimeOffset.MinValue;
    private static MarketChartById _btcHistory;
    private static DateTimeOffset _firstHistoryTime = DateTimeOffset.MaxValue;

    public string Source => ListingSource.CoinGecko;

    protected override string ApiStateConfigName => "Api.State.CoinGecko";

    protected override string ApiMessageTitle => "Market service CoinGecko";

    protected override string ApiMessageContent => "The market data API for CoinGecko recieved an error.\r\r\n{0}";

    public CoinGeckoApi(
        IOptions<CoinGeckoOptions> coinGeckoSettings,
        IAppConfigService appConfigService,
        INotificationManager notificationManager,
        ILogger<CoinGeckoApi> logger
        ) : base(appConfigService, notificationManager, logger)
    {
        // Make the api impersonate a browser, otherwise market data will be prohibited.
        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Remove("User-Agent");
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/111.0");
        _coinGeckoOptions = coinGeckoSettings.Value;
        _coinGeckoClient = new(httpClient, _coinGeckoOptions.ApiKey);
    }

    /// <summary>
    /// Load listed coins. This is cached in a static var and only loaded ones 
    /// a day to prevent using too many API call credits.
    /// </summary>
    /// <returns></returns>
    private async Task<bool> LoadCoinsList()
    {
        if (_lastCoinsUpdateTime > DateTimeOffset.UtcNow.AddHours(-CACHE_TIME_IN_HOURS)) return true;

        // Get the list of coins
        var newcoins = await ApiRequestAsync(async () => await _coinGeckoClient.CoinsClient.GetCoinList(true));

        if (newcoins != null)
        {
            _coins = newcoins;
            _lastCoinsUpdateTime = DateTimeOffset.UtcNow;
            return true;
        }

        // List is not loaded
        return false;
    }

    /// <summary>
    /// Load listed coins. This is cached in a static var and only loaded ones 
    /// a day to prevent using too many API call credits.
    /// </summary>
    /// <returns></returns>
    private async Task<bool> LoadAssetPlatforms()
    {
        if (_lastPlatformsUpdateTime > DateTimeOffset.UtcNow.AddHours(-CACHE_TIME_IN_HOURS)) return true;

        // Get the list of coins
        var platforms = await ApiRequestAsync(async () => await _coinGeckoClient.CoinsClient.GetAssetPlatforms());

        if (platforms != null)
        {
            _platforms = platforms;
            _lastPlatformsUpdateTime = DateTimeOffset.UtcNow;
            return true;
        }

        // List is not loaded
        return false;
    }

    private async Task<bool> LoadBtcHistory(DateTimeOffset startDate)
    {
        if (_firstHistoryTime < startDate) return true;

        // And the USD listings for the requested coin
        var newBtcHistory = await ApiRequestAsync(async () => await _coinGeckoClient.CoinsClient
            .GetMarketChartRangeByCoinId(
                "bitcoin",
                "usd",
                startDate.ToUnixTimeSeconds().ToString(),
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
                ));

        if (newBtcHistory != null)
        {
            _btcHistory = newBtcHistory;
            _firstHistoryTime = startDate;
            return true;
        }

        // List is not loaded
        return false;
    }

    /// <summary>
    /// Get the CoinGecko currency id for the given currency symbol.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    private async Task<string> GetCoinId(string symbol) => (await GetCoin(symbol))?.Id;

    /// <summary>
    /// Get the CoinGecko currency for the given currency symbol.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    private async Task<CoinList> GetCoin(string symbol) => !string.IsNullOrEmpty(symbol) && await LoadCoinsList()
        ? _coins.FirstOrDefault(i => string.Equals(i.Symbol, symbol, StringComparison.OrdinalIgnoreCase))
        : null;

    /// <summary>
    /// Get the CoinGecko currencies for the given currency symbols.
    /// </summary>
    /// <param name="symbols"></param>
    /// <returns></returns>
    private async Task<IEnumerable<CoinList>> GetCoins(IEnumerable<string> symbols) => await LoadCoinsList()
        ? _coins.Where(i => symbols.Any(s => s.Equals(i.Symbol, StringComparison.OrdinalIgnoreCase)))
        : null;

    /// <summary>
    /// Returns a list of CoinGecko currency id's that are available in the API.
    /// </summary>
    /// <param name="currencies"></param>
    /// <returns></returns>
    private async Task<List<string>> FilterCurrencies(IEnumerable<MarketCryptoCurrency> currencies)
    {
        List<string> result = new();

        foreach (var cryptocurrency in currencies)
        {
            var coinId = await GetCoinId(cryptocurrency.Symbol);
            if (!string.IsNullOrEmpty(coinId))
                result.Add(coinId);
        }

        return result;
    }

    private async Task<MarketChartById> GetBtcHistory(DateTimeOffset startDate) =>
        await LoadBtcHistory(startDate)
        ? _btcHistory
        : null;

    public async Task<byte[]> GetIcon(string symbol, CancellationToken cancellationToken)
    {
        var coinId = await GetCoinId(symbol);

        if (string.IsNullOrEmpty(coinId)) return null;

        var coinMarkets = await ApiRequestAsync(async () => await _coinGeckoClient.CoinsClient
            .GetCoinMarkets(
                "usd",
                new string[] { coinId },
                "",
                null,
                null,
                false,
                string.Empty,
                string.Empty), cancellationToken);

        if (coinMarkets != null && coinMarkets.Count > 0)
        {
            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Accept", "image/png");

            var response = await httpClient.GetAsync(coinMarkets.First().Image, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                Image img = Image.Load(bytes);

                return ImageHelper.ConvertToPng(img, new Size(64, 64));
            }
        }

        return null;
    }

    public async Task<IEnumerable<MarketListing>> GetHistoricalListings(MarketCryptoCurrency crypto, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        List<MarketListing> listings = new();

        // Get all market values from CoinGecko and add them to the listings
        // list to add in the database.
        // Symbol is not found in the API, return true to remove the
        // ticker from the history list.
        var coinId = await GetCoinId(crypto.Symbol);
        if (string.IsNullOrEmpty(coinId)) return listings;

        var startDateOffset = new DateTimeOffset(startDate);
        var endDateOffset = new DateTimeOffset(endDate);

        // Get the BTC history to calculate the BTC rate for the USD listing
        var historyBtc = await GetBtcHistory(startDateOffset);
        if (historyBtc == null) return listings;

        // And the USD listings for the requested coin
        var history = await ApiRequestAsync(async () => await _coinGeckoClient.CoinsClient
            .GetMarketChartRangeByCoinId(
                coinId,
                "usd",
                startDateOffset.ToUnixTimeSeconds().ToString(),
                endDateOffset.ToUnixTimeSeconds().ToString()
                ), cancellationToken);

        foreach (var price in history.Prices)
        {
            DateTime priceDate = DateTimeHelper.JavaTimeStampToDateTime((double)price[0]);

            var btcPriceOnDate = GetValueForDate(historyBtc.Prices, (decimal)price[0]);
            var volume = GetValueForDate(history.TotalVolumes, (decimal)price[0]);
            var marketCap = GetValueForDate(history.MarketCaps, (decimal)price[0]);

            // Add listing
            listings.Add(new MarketListing()
            {
                Symbol = crypto.Symbol,
                Source = Source,
                TimeStamp = priceDate.Date.ToUniversalTime(),
                CmcRank = 0,
                CirculatingSupply = 0,
                TotalSupply = 0,
                MaxSupply = 0,
                USDPrice = (decimal)price[1],
                BTCPrice = btcPriceOnDate > 0 ? (decimal)price[1] / btcPriceOnDate : 0,
                Volume_24h = volume,
                PercentChange_1h = 0,
                PercentChange_24h = 0,
                PercentChange_7d = 0,
                MarketCap = marketCap
            });

            if (cancellationToken.IsCancellationRequested) return null;
        }

        return listings;

        static decimal GetValueForDate(decimal?[][] items, decimal date)
        {
            const int secondsInDay = 24 * 60 * 60;
            var day = Math.Floor(date / secondsInDay);

            return items
                .Where(i => i[0] != null && i[1] != null && Math.Floor((decimal)i[0] / secondsInDay) == day)
                .Select(i => (decimal)i[1])
                .FirstOrDefault(0);
        }
    }

    public async Task<IEnumerable<MarketListing>> GetLatestListings(IEnumerable<MarketCryptoCurrency> cryptos, CancellationToken cancellationToken)
    {
        List<MarketListing> listings = new();

        // First find all the currencies available on CoinGecko so we query
        // only existing ones. The CoinGeckoCache stores a complete list of
        // coins the API lists so it doesn't has to be queried all everytime we
        // run the update.
        List<string> coinGeckoIds = new() { "bitcoin" }; // Always start with bitcoin to get the BTC price first.
        coinGeckoIds.AddRange(await FilterCurrencies(cryptos));

        decimal btcPrice = 0;

        while (coinGeckoIds.Count > 0 && !cancellationToken.IsCancellationRequested)
        {
            var fetchSet = coinGeckoIds.Take(COINGECKO_MARKETS_COUNT_PER_FETCH).ToArray();
            coinGeckoIds.RemoveRange(0, fetchSet.Length);

            // Get the list of coins
            var markets = await ApiRequestAsync(async () => await _coinGeckoClient.CoinsClient
                .GetCoinMarkets(
                    "usd",                  // vsCurrency
                    fetchSet,               // ids
                    CoinGeckoAPI.Parameters.OrderField.MarketCapDesc, // order
                    COINGECKO_MARKETS_COUNT_PER_FETCH, // per_page
                    1,                      // page
                    false,                  // sparkline
                    "1h,24h,7d",            // priceChangePercentage
                    string.Empty            // category
                    ), cancellationToken);

            if (markets == null || markets.Count == 0) return null;

            // If the BTC price is not yet set, do that first
            if (btcPrice.Equals(0))
            {
                var btcMarket = markets.Where(m => m.Symbol.Equals("BTC", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                btcPrice = (decimal)btcMarket?.CurrentPrice;
            }
            if (btcPrice.Equals(0)) continue;

            foreach (var market in markets)
            {
                // First find the currency
                var crypto = cryptos.Where(c => c.Symbol.Equals(market.Symbol, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (crypto == null) continue;

                // Add listing
                listings.Add(new MarketListing()
                {
                    Symbol = crypto.Symbol,
                    Source = Source,
                    TimeStamp = (DateTime)market.LastUpdated?.UtcDateTime,
                    CmcRank = (int)market.MarketCapRank.GetValueOrDefault(0),
                    CirculatingSupply = market.CirculatingSupply.GetValueOrDefault(0),
                    TotalSupply = market.CirculatingSupply.GetValueOrDefault(0),
                    MaxSupply = market.TotalSupply.GetValueOrDefault(0),
                    USDPrice = market.CurrentPrice.GetValueOrDefault(0),
                    BTCPrice = market.CurrentPrice.GetValueOrDefault(0) / btcPrice,
                    Volume_24h = market.TotalVolume.GetValueOrDefault(0),
                    PercentChange_1h = market.PriceChangePercentage1HInCurrency.GetValueOrDefault(0),
                    PercentChange_24h = market.PriceChangePercentage24HInCurrency.GetValueOrDefault(0),
                    PercentChange_7d = market.PriceChangePercentage7DInCurrency.GetValueOrDefault(0),
                    MarketCap = market.MarketCap.GetValueOrDefault(0)
                });
            }
        }

        return listings;
    }

    public Task<IEnumerable<MarketCryptoCurrency>> GetCryptoCurrencies(int startIndex, int endIndex, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetCurrencyRank(string symbol, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<MarketTokenContact>> GetTokenContracts(IEnumerable<string> symbols, CancellationToken cancellationToken)
    {
        List<MarketTokenContact> result = new();

        var coins = await GetCoins(symbols);

        // Now find the platforms from the coinId
        if (coins != null && await LoadAssetPlatforms())
        {
            foreach (var coin in coins)
            {
                foreach (var platform in coin.Platforms)
                {
                    var network = _platforms.SingleOrDefault(c => c.Id.Equals(platform.Key));
                    if (network != null)
                    {
                        result.Add(new()
                        {
                            Symbol = coin.Symbol,
                            Network = network.Name,
                            // NetworkToken ,_ Not available
                            ChainId = network.ChainId,
                            ContractAddress = platform.Value
                        });
                    }
                }
            }
        }

        return result;
    }
}
