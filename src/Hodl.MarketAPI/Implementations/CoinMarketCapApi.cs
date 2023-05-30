using CoinMarketCapAPI.Clients;
using CoinMarketCapAPI.Models.CryptoCurrency;
using CoinMarketCapAPI.Parameters;
using Hodl.Framework;
using Hodl.MarketAPI.Models;
using Hodl.Utils;

namespace Hodl.MarketAPI.Implementations;

public class CoinMarketCapApi : ExternalApi, ICryptoMarketApi
{
    private const int CACHE_TIME_IN_HOURS = 24;
    private const int COINMARKETCAP_QUOTES_COUNT_PER_FETCH = 100;
    private const string ISO_8601_DATE = "yyyy-MM-dd";

    private readonly CoinmarketCapOptions _coinMarketCapOptions;
    private readonly CoinMarketCapClient _coinMarketCapClient;

    private static IList<IdMapData> _coins;
    private static DateTime _lastUpdateTime = DateTime.MinValue;

    public string Source => ListingSource.CoinMarketCap;

    protected override string ApiStateConfigName => "Api.State.CoinMarketCap";

    protected override string ApiMessageTitle => "Market service CoinMarketCap";

    protected override string ApiMessageContent => "The market data API for CoinMarketCap recieved an error.\r\r\n{0}";

    public CoinMarketCapApi(
        IOptions<CoinmarketCapOptions> cmcOptions,
        IAppConfigService appConfigService,
        INotificationManager notificationManager,
        ILogger<CoinMarketCapApi> logger
        ) : base(appConfigService, notificationManager, logger)
    {
        _coinMarketCapOptions = cmcOptions.Value;
        _coinMarketCapClient = new(ApiEnvironment.Pro, _coinMarketCapOptions.ApiKey);
    }

    /// <summary>
    /// Load listed coins. This is cached in a static var and only loaded ones 
    /// a day to prevent using too many API call credits.
    /// </summary>
    /// <returns></returns>
    private async Task<bool> LoadCoinsList()
    {
        if (_lastUpdateTime > DateTime.UtcNow.AddHours(-CACHE_TIME_IN_HOURS)) return true;

        // Get the list of coins
        var response = await ApiRequestAsync(async () => await _coinMarketCapClient.CryptoCurrencyClient
            .GetIdMap(Array.Empty<string>()));

        if (response != null && response.Status.ErrorCode.Equals(0))
        {
            // Clear old data
            _coins?.Clear();

            _coins = new List<IdMapData>(response.Data);
            _lastUpdateTime = DateTime.UtcNow;

            return true;
        }

        // List is not loaded
        return false;
    }

    /// <summary>
    /// Get the CoinmarketCap currency id for the given currency symbol.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    private async Task<long> GetCoinId(string symbol) => !string.IsNullOrEmpty(symbol) && await LoadCoinsList()
        ? _coins.Where(i => i.IsActive).FirstOrDefault(i => string.Equals(i.Symbol, symbol, StringComparison.OrdinalIgnoreCase))?.Id ?? 0
        : 0;

    /// <summary>
    /// Get the CoinmarketCap currency id's for the given currency symbols.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    private async Task<long[]> GetCoinIds(IEnumerable<string> symbols) => await LoadCoinsList()
        ? _coins.Where(i => i.IsActive && symbols.Any(s => s.Equals(i.Symbol, StringComparison.OrdinalIgnoreCase))).Select(i => i.Id).ToArray()
        : null;

    /// <summary>
    /// Returns a list of CoinMarketCap currency id's that are available in the API.
    /// </summary>
    /// <param name="currencies"></param>
    /// <returns></returns>
    private async Task<List<long>> FilterCurrencies(IEnumerable<MarketCryptoCurrency> currencies)
    {
        List<long> result = new();

        foreach (var cryptocurrency in currencies)
        {
            var coinId = await GetCoinId(cryptocurrency.Symbol);
            if (coinId != 0)
                result.Add(coinId);
        }

        return result;
    }

    public async Task<byte[]> GetIcon(string symbol, CancellationToken cancellationToken)
    {
        var coinId = await GetCoinId(symbol);
        if (coinId == 0) return null;

        // Get the list of coins
        var metadata = await ApiRequestAsync(async () => await _coinMarketCapClient.CryptoCurrencyClient
            .GetMetaData(new long[] { coinId }), cancellationToken);

        if (metadata != null && metadata.Status.ErrorCode.Equals(0))
        {
            // Http client for retrieving the image
            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Accept", "image/png");

            // Add the <Uri> type Logo metadata of each listed coin BY THE MARKETCAP RANKING ORDER
            var coinInfo = metadata.Data[coinId];
            // Dealing with the obtained "logo" data
            var response = await httpClient.GetAsync(coinInfo.Logo, cancellationToken);

            // Basically checking if the HTTP response code is not within the 200-299 range, in which case is false)
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
        // Symbol is not found in the API, return true to remove the
        // ticker from the history list.
        var coinId = await GetCoinId(crypto.Symbol);
        if (coinId == 0) return null;

        List<MarketListing> listings = new();

        // Get all market values from CoinMarketCap and add them to the listings
        // list to add in the database.

        // Get the quotes in USD and BTC for the requested time period
        // string id, string symbol, string timeStart, string timeEnd, int? count, string interval, string[] convert
        var historicalQuotes = await ApiRequestAsync(async () => await _coinMarketCapClient.CryptoCurrencyClient
            .GetQuotesHistorical(
                id: coinId,
                timeStart: startDate.ToString(ISO_8601_DATE),
                timeEnd: endDate.ToString(ISO_8601_DATE),
                count: null,
                interval: "daily",
                convert: new string[] { "USD", "BTC" }), cancellationToken);

        // No http error, but no result or an error in the result
        if (historicalQuotes == null)
            return listings;

        // Some error, log it
        if (!historicalQuotes.Status.ErrorCode.Equals(0))
        {
            _logger.LogWarning("Error while getting historical quotes from CoinMarketCap. Returned error code {errorcode}.\r\n{errormessage}",
                historicalQuotes.Status.ErrorCode,
                historicalQuotes.Status.ErrorMessage);
            return listings;
        }

        foreach (var quotes in historicalQuotes.Data.Quotes)
        {
            // Add listing
            listings.Add(new MarketListing()
            {
                Symbol = crypto.Symbol,
                Source = Source,
                TimeStamp = quotes.Timestamp.UtcDateTime,
                CmcRank = 0,
                CirculatingSupply = 0,
                TotalSupply = 0,
                MaxSupply = 0,
                USDPrice = (decimal)quotes.Quote["USD"].Price,
                BTCPrice = (decimal)quotes.Quote["BTC"].Price,
                Volume_24h = quotes.Quote["USD"].Volume24H,
                PercentChange_1h = 0,
                PercentChange_24h = 0,
                PercentChange_7d = 0,
                MarketCap = (decimal)quotes.Quote["USD"].MarketCap
            });

            if (cancellationToken.IsCancellationRequested) return null;
        }

        return listings;
    }

    public async Task<IEnumerable<MarketListing>> GetLatestListings(IEnumerable<MarketCryptoCurrency> cryptos, CancellationToken cancellationToken)
    {
        List<MarketListing> listings = new();

        // First find all the currencies available on CoinMarketCap so we query
        // only existing ones. The CoinMarketCapCache stores a complete list of
        // coins the API lists so it doesn't has to be queried all everytime we
        // run the update.
        List<long> coinMarketCapIds = new(1); // Always start with bitcoin to get the BTC price first.
        coinMarketCapIds.AddRange(await FilterCurrencies(cryptos));

        // Get all market values from CoinGecko and add them to the listings
        // list to add in the database.
        decimal btcPrice = 0;
        int apiCreditsUsed = 0;

        while (coinMarketCapIds.Count > 0
            && apiCreditsUsed < _coinMarketCapOptions.DailyCredits / (24 * 12) // Runs every 5 minutes, so 12 times every hour
            && !cancellationToken.IsCancellationRequested)
        {
            var fetchSet = coinMarketCapIds.Take(COINMARKETCAP_QUOTES_COUNT_PER_FETCH).ToArray();
            coinMarketCapIds.RemoveRange(0, fetchSet.Length);

            // Get the list of coins
            var response = await ApiRequestAsync(async () => await _coinMarketCapClient.CryptoCurrencyClient
                .GetQuotesLatest(
                    fetchSet.Select(i => i).ToArray(),
                    new string[] { "USD" }),
                cancellationToken);

            if (response == null) return null;
            if (!response.Status.ErrorCode.Equals(0)) continue;

            apiCreditsUsed += (int)response.Status.CreditCount;
            var quotes = response.Data.Values;

            // If the BTC price is not yet set, do that first
            if (btcPrice.Equals(0))
            {
                var btcQuote = quotes.Where(q => q.Symbol.Equals("BTC", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                btcPrice = (decimal)(btcQuote != null ? btcQuote.Quote["USD"].Price : 0);
            }
            if (btcPrice.Equals(0)) continue;

            foreach (var quote in quotes)
            {
                // First find the currency
                var crypto = cryptos.Where(c => c.Symbol.Equals(quote.Symbol, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (crypto == null) continue;

                // Add listing
                listings.Add(new MarketListing()
                {
                    Symbol = crypto.Symbol,
                    Source = Source,
                    TimeStamp = quote.Quote["USD"].LastUpdated.UtcDateTime.GetValueOrDefault(DateTime.UtcNow),
                    CmcRank = (int)quote.CmcRank,
                    CirculatingSupply = quote.CirculatingSupply,
                    TotalSupply = quote.TotalSupply,
                    MaxSupply = quote.MaxSupply.GetValueOrDefault(0),
                    USDPrice = quote.Quote["USD"].Price,
                    BTCPrice = quote.Quote["USD"].Price / btcPrice,
                    Volume_24h = quote.Quote["USD"].Volume24H.GetValueOrDefault(0),
                    PercentChange_1h = quote.Quote["USD"].PercentChange1H.GetValueOrDefault(0),
                    PercentChange_24h = quote.Quote["USD"].PercentChange24H.GetValueOrDefault(0),
                    PercentChange_7d = quote.Quote["USD"].PercentChange7D.GetValueOrDefault(0),
                    MarketCap = quote.Quote["USD"].MarketCap,
                });
            }
        }

        return listings;
    }

    public async Task<IEnumerable<MarketCryptoCurrency>> GetCryptoCurrencies(int startIndex, int endIndex, CancellationToken cancellationToken)
    {
        List<MarketCryptoCurrency> result = new();

        startIndex = Math.Max(1, startIndex);
        endIndex = Math.Max(startIndex, endIndex);

        var coins_latest_listing_response = await ApiRequestAsync(async () => await _coinMarketCapClient.CryptoCurrencyClient
            .GetListingLatest(
                startIndex,
                endIndex,
                new[] { "USD" },
                SortField.MarketCap,
                SortDirection.Desc,
                CryptoCurrencyType.All), cancellationToken);

        // Checking response of GetListingLatest
        if (coins_latest_listing_response == null || coins_latest_listing_response.Status.ErrorMessage != null)
            throw new Exception("CMC GetListingLatest Response Error");

        // List declarations               
        var coins_latest_listing = new List<ListingLatestData>(coins_latest_listing_response.Data);
        List<long> coinIdsList = new();

        // Add the CMC listed coin symbols by CMC's employed ranking (descending marketcap)
        for (int i = 0; i < coins_latest_listing.Count; i++)
        {
            coinIdsList.Add(coins_latest_listing[i].Id);
        }

        // Fetching the metadata (i.e. Name, Symbol & Logo) for all the listed crypto currencies retrieved
        var coins_MetaData = await ApiRequestAsync(async () => await _coinMarketCapClient.CryptoCurrencyClient
            .GetMetaData(coinIdsList.ToArray()), cancellationToken);

        // Checking response of GetListingLatest
        if (coins_MetaData == null || coins_MetaData.Status.ErrorMessage != null)
            throw new Exception("CMC GetMetaData Respone Error");

        // Add the <Uri> type Logo metadata of each listed coin BY THE MARKETCAP RANKING ORDER
        foreach (var symbol_i in coinIdsList)
        {
            // Instantiating a CryptoCurrency domain model cls instance with the data for the ith coin
            result.Add(new()
            {
                Symbol = coins_MetaData.Data[symbol_i].Symbol,
                Name = coins_MetaData.Data[symbol_i].Name,
                Active = true,
                IsStableCoin = coins_MetaData.Data[symbol_i].Tags?.Contains("stablecoin") ?? false
            });
        }

        return result;
    }

    public async Task<int> GetCurrencyRank(string symbol, CancellationToken cancellationToken)
    {
        var coinId = await GetCoinId(symbol);

        if (coinId.Equals(0)) return -1;

        var cmc_latest_quotes = await ApiRequestAsync(async () => await _coinMarketCapClient.CryptoCurrencyClient
            .GetQuotesLatest(new long[] { coinId }, new string[] { "USD" }), cancellationToken);

        if (cmc_latest_quotes != null && cmc_latest_quotes.Status.ErrorCode.Equals(0))
        {
            return Convert.ToInt32(cmc_latest_quotes.Data[coinId].CmcRank);
        }

        return -1;
    }

    public async Task<IEnumerable<MarketTokenContact>> GetTokenContracts(IEnumerable<string> symbols, CancellationToken cancellationToken)
    {
        List<MarketTokenContact> result = new();

        var coinIds = await GetCoinIds(symbols);

        if (coinIds == null || !coinIds.Any()) return result;

        var coinMetadata = await ApiRequestAsync(async () => await _coinMarketCapClient.CryptoCurrencyClient
            .GetMetaData(coinIds), cancellationToken);

        foreach (var coinId in coinIds)
        {
            if (coinMetadata.Data.TryGetValue(coinId, out CryptoCurrencyInfoData coinInfoData))
            {
                foreach (var contract in coinInfoData.ContractAddresses)
                {
                    result.Add(new()
                    {
                        Symbol = coinInfoData.Symbol,
                        ContractAddress = contract.Address,
                        Network = contract.Platform.Coin.Name,
                        NetworkToken = contract.Platform.Coin.Symbol.ToUpperInvariant(),
                    });
                }
            }
        }

        return result;
    }
}
