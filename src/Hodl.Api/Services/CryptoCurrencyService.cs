using Hodl.Api.FilterParams;
using Hodl.MarketAPI;
using Hodl.MarketAPI.Models;
using Microsoft.Extensions.Options;

namespace Hodl.Api.Services;

public class CryptoCurrencyService : ICryptoCurrencyService
{
    private const string BTC_GUID = "fb3194c2-e732-4219-89b9-bc63eea2a861";
    private const string IMPORT_TOKEN_CONTRACT_PROCESSNAME = "TokenContract.Import.Task";
    private const string PREFERED_SOURCE = ListingSource.CoinMarketCap;

    private static readonly Guid _btcGuid = Guid.Parse(BTC_GUID);

    private readonly HodlDbContext _db;
    private readonly int _listingUpdateInterval;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppConfigService _appConfigService;
    private readonly IMapper _mapper;
    private readonly IErrorManager _errorManager;

    public CryptoCurrencyService(
        HodlDbContext dbContext,
        IOptions<AppDefaults> settings,
        IServiceProvider serviceProvider,
        IAppConfigService appConfigService,
        IMapper mapper,
        IErrorManager errorManager)
    {
        _db = dbContext;
        _listingUpdateInterval = settings.Value.CryptoListingUpdateInSeconds / 60;
        _serviceProvider = serviceProvider;
        _appConfigService = appConfigService;
        _mapper = mapper;
        _errorManager = errorManager;
    }

    public Guid BtcGuid => _btcGuid;

    public string PreferedListingSource => PREFERED_SOURCE;

    #region CryptoCurrencies

    public async Task<CryptoCurrency> GetBtcCryptoCurrencyAsync(CancellationToken cancellationToken = default)
    {
        return await _db.CryptoCurrencies.SingleAsync(c => c.Id.ToString().Equals(BTC_GUID), cancellationToken);
    }

    public async Task<IList<CryptoCurrency>> GetUsedCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        var cryptoIds = _db.Holdings
            .Where(h => h.CryptoId != null && h.PeriodClosedDateTime == null)
            .Select(h => h.CryptoId)
            .ToHashSet();

        return await _db.CryptoCurrencies
            .Where(c => c.Active && cryptoIds.Contains(c.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<PagingModel<CryptoCurrency>> GetUsedCurrenciesAsync(int page, int? itemsPerPage, CancellationToken cancellationToken = default)
    {
        var query = _db.Holdings
            .AsNoTracking()
            .Where(h => h.CryptoId != null && h.PeriodClosedDateTime == null)
            .Select(h => h.CryptoCurrency)
            .Where(c => c.Active)
            .Distinct()
            .OrderBy(c => c.Name);

        // Gotta first declare this var (with the correct type) due to scoping matteres
        return await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, cancellationToken);
    }

    public async Task<CryptoCurrency> GetCryptoCurrencyBySymbol(string symbol, CancellationToken cancellationToken = default)
    {
        return await _db.CryptoCurrencies
            .Where(c => c.Symbol == symbol)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CryptoCurrency> GetCryptoCurrencyAsync(Guid cryptoId, CancellationToken cancellationToken = default)
    {
        return await _db.CryptoCurrencies
            .Where(c => c.Id.Equals(cryptoId))
            .AsSingleQuery()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagingModel<CryptoCurrency>> Search(string search, int page, int? itemsPerPage, CancellationToken cancellationToken = default)
    {
        // Filters
        // To make an OR statement, create two separate filter queries and
        // combine the results with a union. Do a disctinct in the end to make
        // sure no items are returned twice.
        var filter1 = new CryptoFilterParams
        {
            Symbol = search
        };
        var filter2 = new CryptoFilterParams
        {
            Name = search
        };

        var query = _db.CryptoCurrencies
            .AsNoTracking()
            .Filter(filter1)
            .Union(_db.CryptoCurrencies
                .AsNoTracking()
                .Filter(filter2))
            .Distinct()
            .OrderBy(c => c.Name);

        return await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, cancellationToken);
    }

    public async Task AddCryptoCurrency(CryptoCurrency crypto, CancellationToken cancellationToken = default)
    {
        // Check if unique condition is matched: nameof(Symbol), nameof(Name)
        if (await _db.CryptoCurrencies.AnyAsync(c => c.Symbol == crypto.Symbol && c.Name == crypto.Name, cancellationToken))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = "Symbol + Name",
                Code = ErrorCodesStore.DuplicateKey,
                Description = "The crypto currency already exists."
            });

        // Raise error result when any errors occured
        _errorManager.ThrowOnErrors();

        // No exceptions, so proceed with the actual handling of the request
        await _db.CryptoCurrencies.AddAsync(crypto, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        // And trigger background process for the listing history retrieval.
        await TriggerListingsHistory(crypto.Symbol, cancellationToken);
    }

    public async Task UpdateCryptoCurrency(CryptoCurrency crypto, CancellationToken cancellationToken = default)
    {
        // Check if unique condition is matched: nameof(Symbol), nameof(Name)
        if (await _db.CryptoCurrencies.AnyAsync(c => c.Symbol == crypto.Symbol && c.Name == crypto.Name && c.Id != crypto.Id, cancellationToken))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = "Symbol + Name",
                Code = ErrorCodesStore.DuplicateKey,
                Description = "The crypto currency already exists."
            });

        _errorManager.ThrowOnErrors();

        _db.CryptoCurrencies.Update(crypto);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCurrency(Guid cryptoId, CancellationToken cancellationToken = default)
    {
        var storedCrypto = await GetCryptoCurrencyAsync(cryptoId, cancellationToken) ??
            throw new NotFoundException($"Cryptocurrency with id {cryptoId} not found");

        // Do checks for existing holdings or orders and return nice error messages.
        if (await _db.Funds.AnyAsync(f => f.PrimaryCryptoCurrencyId == cryptoId, cancellationToken))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Fund.PrimaryCryptoCurrencyId),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "The crypto currency is referenced as from a fund as primary crypto currency so can not be deleted."
            });
        if (await _db.Holdings.AnyAsync(c => c.CryptoId == cryptoId, cancellationToken))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Holding.CryptoId),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "The crypto currency is referenced from a holding so can not be deleted."
            });

        if (await _db.Orders.AnyAsync(c => c.QuoteAssetId == cryptoId, cancellationToken) ||
            await _db.Orders.AnyAsync(c => c.BaseAssetId == cryptoId, cancellationToken))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Order.BaseAssetId) + "/" + nameof(Order.QuoteAssetId),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "The crypto currency is referenced from an order so can not be deleted."
            });

        _errorManager.ThrowOnErrors();

        _db.CryptoCurrencies.Remove(storedCrypto);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the cryptocurrency with the listing. This can be used when a 
    /// referencing currency is made, an alternative to the original token.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<CryptoCurrency> GetListingCryptoId(Guid cryptoId, CancellationToken cancellationToken = default)
    {
        var crypto = await GetCryptoCurrencyAsync(cryptoId, cancellationToken);

        return crypto.ListingCryptoId == null
            ? crypto
            : await GetListingCryptoId((Guid)crypto.ListingCryptoId, cancellationToken);
    }

    public async Task<int> ImportCoins(int startIndex, int endIndex, CancellationToken cancellationToken = default)
    {
        // Import the most important currencies from the market data providers
        int coins_added_counter = 0;

        foreach (var marketApi in _serviceProvider.GetServices<ICryptoMarketApi>())
        {
            try
            {
                var cryptos = await marketApi.GetCryptoCurrencies(startIndex, endIndex, cancellationToken);

                foreach (var crypto in cryptos)
                {
                    if (!await _db.CryptoCurrencies.AnyAsync(c => c.Symbol == crypto.Symbol && c.Name == crypto.Name, cancellationToken))
                    {
                        // Now also get the icon data
                        crypto.Icon = await marketApi.GetIcon(crypto.Symbol, cancellationToken);
                        await _db.CryptoCurrencies.AddAsync(_mapper.Map<CryptoCurrency>(crypto), cancellationToken);
                    }
                }
                coins_added_counter += await _db.SaveChangesAsync(cancellationToken);
            }
            catch (NotImplementedException) { }
        }

        return coins_added_counter;
    }

    #endregion

    #region Listings

    public async Task<Listing> GetFirstListing(Guid cryptoId, CancellationToken cancellationToken = default)
    {
        var listingCrypto = await GetListingCryptoId(cryptoId, cancellationToken);

        return await _db.Listings
            .Where(l => l.CryptoId.Equals(listingCrypto.Id))
            .OrderBy(l => l.TimeStamp)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Listing> GetMostRecentListing(Guid cryptoId, string preferedSource, CancellationToken cancellationToken = default)
    {
        var listingCrypto = await GetListingCryptoId(cryptoId, cancellationToken);

        // Select CoinMarketCap listing or fallback to an other source
        return await _db.Listings
            .Where(l => l.CryptoId.Equals(listingCrypto.Id) && l.Source.Equals(preferedSource) && l.TimeStamp > DateTime.UtcNow.AddHours(-12))
            .OrderByDescending(l => l.TimeStamp)
            .FirstOrDefaultAsync(cancellationToken)
            ??
            await _db.Listings
            .Where(l => l.CryptoId == listingCrypto.Id)
            .OrderByDescending(l => l.TimeStamp)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetMostRecentListings(Guid cryptoId, CancellationToken cancellationToken = default)
    {
        var listingCrypto = await GetListingCryptoId(cryptoId, cancellationToken);

        return await _db.Listings
            .AsNoTracking()
            .Where(l => l.CryptoId.Equals(listingCrypto.Id))
            .OrderByDescending(l => l.TimeStamp)
            .Take(100)
            .ToArrayAsync(cancellationToken);
    }

    /// <summary>
    /// Return the listing for the given date. Prefer CoinmarketCap, when not 
    /// available for the requested date, try to select the CoinGecko listing.
    /// If still no listing is available, fall back on the last available listing.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="datetime"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Listing> GetListingByDate(Guid cryptoId, DateTimeOffset datetime, string preferedSource, bool importHistory, CancellationToken cancellationToken = default)
    {
        // First get the 
        var listingCrypto = await GetListingCryptoId(cryptoId, cancellationToken);

        // When the import flag is true, check if there is listing available
        // with the past 24 hours. If not, import history data.
        if (importHistory &&
            !await _db.Listings.AnyAsync(l => l.CryptoId.Equals(listingCrypto.Id) &&
                l.TimeStamp < datetime.AddMinutes(1).UtcDateTime &&
                l.TimeStamp > datetime.AddHours(-24).UtcDateTime,
                cancellationToken))
        {
            // Import historical data from market API
            _ = await AddListingsHistory(listingCrypto, datetime.UtcDateTime, cancellationToken);
        }

        // First try for a manual input on the same date (NAV value), when not
        // available pick the prefered source. When still no value can be
        // found, pick any previous listing. If still no listing is available,
        // get the first one available.
        return await _db.Listings
            .Where(l => l.CryptoId.Equals(listingCrypto.Id) &&
                l.Source.StartsWith("NAV") &&
                l.TimeStamp.Date < datetime.AddMinutes(1).UtcDateTime &&
                l.TimeStamp > datetime.AddHours(-1).UtcDateTime)
            .OrderByDescending(r => r.TimeStamp)
            .FirstOrDefaultAsync(cancellationToken)
            ??
            await _db.Listings
            .Where(l => l.CryptoId.Equals(listingCrypto.Id) &&
                l.Source.Equals(preferedSource) &&
                l.TimeStamp < datetime.AddMinutes(1).UtcDateTime &&
                l.TimeStamp > datetime.AddHours(-24).UtcDateTime)
            .OrderByDescending(r => r.TimeStamp)
            .FirstOrDefaultAsync(cancellationToken)
            ??
            await _db.Listings
            .Where(l => l.CryptoId.Equals(listingCrypto.Id) && l.TimeStamp <= datetime.UtcDateTime)
            .OrderByDescending(r => r.TimeStamp)
            .FirstOrDefaultAsync(cancellationToken)
            ??
            await GetFirstListing(cryptoId, cancellationToken);
    }

    public async Task<PagingModel<Listing>> GetListings(
        Guid cryptoId,
        DateTime dateFrom,
        DateTime dateTo,
        int page,
        int? itemsPerPage,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Listings
            .AsNoTracking()
            .Where(r => r.CryptoId.Equals(cryptoId) && r.TimeStamp >= dateFrom && r.TimeStamp <= dateTo)
            .OrderByDescending(r => r.TimeStamp);

        // Create a paged resultset
        return await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, cancellationToken);
    }

    public async Task<Listing> GetListingById(Guid cryptoId, long id, CancellationToken cancellationToken = default) =>
        await _db.Listings
            .Where(r => r.Id == id && r.CryptoId.Equals(cryptoId))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task UpdateListing(Listing listing, CancellationToken cancellationToken = default)
    {
        _db.Listings.Update(listing);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddListing(Listing listing, CancellationToken cancellationToken = default)
    {
        await _db.Listings.AddAsync(listing, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteListing(Guid cryptoId, long listingId, CancellationToken cancellationToken = default)
    {
        var storedListing = await GetListingById(cryptoId, listingId, cancellationToken) ??
            throw new NotFoundException($"Listing with Id {listingId} not found.");

        // Do checks for existing holdings or NAV's and return nice error messages.
        if (await _db.Holdings.AnyAsync(h => h.ListingId.Equals(listingId), cancellationToken))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, $"Listing with Id {listingId} is referenced so can not be removed.");

        _errorManager.ThrowOnErrors();

        _db.Listings.Remove(storedListing);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> AppendListings(IEnumerable<Listing> listings, CancellationToken cancellationToken = default)
    {
        // First get the current last listings to detect if the new listings
        // are newer than the latest inthe database.
        var lastListing = await _db.Listings
            .Where(l => l.Source.Equals(PreferedListingSource))
            .OrderByDescending(l => l.TimeStamp)
            .FirstOrDefaultAsync(cancellationToken);

        // Save the new items
        await _db.Listings.AddRangeAsync(listings, cancellationToken);
        var added = await _db.SaveChangesAsync(cancellationToken);

        // Update latest prices
        //
        // TODO: For now only CMC ratings will be stored, in the future the
        // prefered holding from the fund should be used.
        if (listings.Any(l => l.TimeStamp >= lastListing.TimeStamp && l.Source.Equals(PreferedListingSource)))
        {
            var latestListings = listings
                .OrderBy(r => r.TimeStamp)
                .DistinctBy(r => r.CryptoId)
                .ToArray();

            // Select all crypto id's where new listings are available for
            var listingCryptoIds = latestListings.Select(l => l.CryptoId).Distinct().ToList();
            // And add the crypto's that reference the listings
            listingCryptoIds.AddRange(await _db.CryptoCurrencies
                .Where(c => c.ListingCryptoId != null && listingCryptoIds.Contains((Guid)c.ListingCryptoId))
                .Select(c => c.Id)
                .Distinct()
                .ToListAsync(cancellationToken));

            // Add the listings to the active holdings
            var holdings = await _db.Holdings
                .Where(h =>
                    h.CryptoId != null &&
                    h.PeriodClosedDateTime == null &&
                    listingCryptoIds.Contains((Guid)h.CryptoId))
                .Include(h => h.CryptoCurrency)
                .ToListAsync(cancellationToken);

            foreach (var holding in holdings)
            {
                var listing = latestListings.FirstOrDefault(l =>
                    l.CryptoId.Equals(holding.CryptoId) ||
                    l.CryptoCurrency.ListingCryptoId.Equals(holding.CryptoId));

                holding.ListingId = listing.Id;
                holding.EndUSDPrice = listing.USDPrice;
                holding.EndBTCPrice = listing.BTCPrice;
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        return added;
    }

    public async Task<bool> AddLatestListings(CancellationToken cancellationToken = default)
    {
        var cryptos = await GetUsedCurrenciesAsync(cancellationToken);
        var marketCryptos = cryptos.Select(c => _mapper.Map<MarketCryptoCurrency>(c));

        int newListings = 0;
        foreach (var cryptoMarketApi in _serviceProvider.GetServices<ICryptoMarketApi>())
        {
            try
            {
                var marketListings = await cryptoMarketApi.GetLatestListings(marketCryptos, cancellationToken);

                if (marketListings != null && marketListings.Any())
                {
                    List<Listing> listings = new();

                    foreach (var marketListing in marketListings)
                    {
                        var crypto = cryptos.FirstOrDefault(c => c.Symbol.Equals(marketListing.Symbol, StringComparison.OrdinalIgnoreCase));

                        if (crypto != null)
                        {
                            var listing = _mapper.Map<Listing>(marketListing);
                            listing.CryptoId = crypto.Id;
                            listings.Add(listing);
                        }
                    }

                    newListings += await AppendListings(listings, cancellationToken);
                }
            }
            catch (NotImplementedException) { }
        }

        return newListings > 0;
    }

    /// <summary>
    /// Retrieves historical listings for the cryptocurrency by the given symbol. 
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="symbol"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> AddListingsHistory(CryptoCurrency crypto, DateTime startDate, CancellationToken cancellationToken = default)
    {
        int newListings = 0;

        foreach (var cryptoMarketApi in _serviceProvider.GetServices<ICryptoMarketApi>())
        {
            // Add the first listing for the currency
            var firstListing = await _db.Listings
                .Where(l => l.CryptoId.Equals(crypto.Id) && l.Source.Equals(cryptoMarketApi.Source))
                .OrderBy(l => l.TimeStamp)
                .FirstOrDefaultAsync(cancellationToken);
            var endDate = firstListing?.TimeStamp ?? DateTime.UtcNow;

            // If the first listing is already older than the required date, skip the process
            if (startDate >= endDate) continue;

            // Rate limiting should be done in the api implementations
            var marketListings = await cryptoMarketApi.GetHistoricalListings(_mapper.Map<MarketCryptoCurrency>(crypto), startDate, endDate, cancellationToken);

            if (marketListings != null && marketListings.Any())
            {
                // Filter the listings so we don't add doubles for the same date
                var filteredListings = marketListings
                    .Where(l => l.TimeStamp < endDate)
                    .Select(l =>
                    {
                        var listing = _mapper.Map<Listing>(l);
                        listing.CryptoId = crypto.Id;
                        return listing;
                    });

                newListings += await AppendListings(filteredListings, cancellationToken);
            }
        }

        return newListings > 0;
    }

    public async Task CleanupListings(int cleanupDelay, CancellationToken cancellationToken = default)
    {
        DateTime until = DateTime.UtcNow.AddDays(-cleanupDelay);

        // TODO: Optimize to limit the selections to 6 months

        var referencedIds = await _db.Holdings
            .Where(h => h.ListingId != null)
            .Select(h => h.ListingId)
            .ToArrayAsync(cancellationToken);

        var removeSelection = await _db.Listings
            .Where(l => l.TimeStamp < until && l.TimeStamp.Minute > 0 && l.TimeStamp.Minute < (60 - _listingUpdateInterval))
            .Where(l => !referencedIds.Contains(l.Id))
            .ToArrayAsync(cancellationToken);

        _db.Listings.RemoveRange(removeSelection);
    }

    /// <summary>
    /// Adds a currency in the history appSetting to let the history routine know
    /// to get historical data points.
    /// </summary>
    /// <param name="currency"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> TriggerListingsHistory(string symbol, CancellationToken cancellationToken = default)
    {
        // Save the new symbol in the config using a lock.
        if (await _appConfigService.WaitForProcessLock(AppConfigs.LISTING_HISTORY_SETTING, AppConfigs.APP_CONFIG_TIMEOUT, cancellationToken))
        {
            try
            {
                string[] symbols = await _appConfigService.GetAppConfigAsync(AppConfigs.LISTING_HISTORY_SYMBOLS, Array.Empty<string>(), cancellationToken);

                // First check if the symbol is not yet in the list.
                if (!symbol.Contains(symbol))
                {
                    // Add the symbol to the list
                    List<string> symbolList = new(symbols)
                    {
                        symbol
                    };
                    // Now update the latest set of symbols
                    await _appConfigService.SetAppConfigAsync(AppConfigs.LISTING_HISTORY_SYMBOLS, symbolList.ToArray(), string.Empty, cancellationToken);
                }
            }
            finally
            {
                await _appConfigService.ReleaseProcessLock(AppConfigs.LISTING_HISTORY_SETTING, cancellationToken);
            }
            return true;
        }

        return false;
    }

    public async Task<IEnumerable<Listing>> SelectListingsBySource(string source, CancellationToken cancellationToken = default) =>
        await _db.Listings
        .Where(l => l.Source.Equals(source))
        .ToArrayAsync(cancellationToken);

    #endregion

    #region Token Contracts

    public async Task<IEnumerable<TokenContract>> ImportTokenContracts(CryptoCurrency crypto, CancellationToken cancellationToken = default) =>
        await ImportTokenContracts(new CryptoCurrency[] { crypto }, cancellationToken);

    public async Task<IEnumerable<TokenContract>> ImportTokenContracts(IEnumerable<CryptoCurrency> cryptos, CancellationToken cancellationToken = default)
    {
        // Request process lock!
        if (!await _appConfigService.RequestProcessLock(IMPORT_TOKEN_CONTRACT_PROCESSNAME, 30, cancellationToken))
            throw new RestException(HttpStatusCode.Conflict, $"An other process is importing token contracts. Only one process can be run at a time.");

        var guids = cryptos.Select(c => c.Id);

        try
        {
            var networks = await _db.BlockchainNetworks
                .AsNoTracking()
                .ToArrayAsync(cancellationToken);

            var currentContracts = await _db.TokenContracts
                .AsNoTracking()
                .Where(c => guids.Contains(c.CryptoId))
                .ToListAsync(cancellationToken);

            List<TokenContract> newContracts = new();

            foreach (var cryptoMarketApi in _serviceProvider.GetServices<ICryptoMarketApi>())
            {
                try
                {
                    var tokenContracts = await cryptoMarketApi.GetTokenContracts(cryptos.Select(c => c.Symbol), cancellationToken);
                    foreach (var contract in tokenContracts)
                    {
                        foreach (var network in networks.Where(n => n.CurrencySymbol.Equals(contract.NetworkToken, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            var crypto = cryptos.SingleOrDefault(c => c.Symbol.Equals(contract.Symbol, StringComparison.InvariantCultureIgnoreCase));
                            if (crypto == null) continue;

                            if (!currentContracts.Any(c => c.BlockchainNetworkId.Equals(network.Id) && c.CryptoId.Equals(crypto.Id)) &&
                                !newContracts.Any(c => c.BlockchainNetworkId.Equals(network.Id) && c.CryptoId.Equals(crypto.Id)))
                            {
                                newContracts.Add(new()
                                {
                                    CryptoId = crypto.Id,
                                    BlockchainNetworkId = network.Id,
                                    ContractAddress = contract.ContractAddress
                                });
                            }
                        }
                    }
                }
                catch (NotImplementedException) { }
            }

            if (newContracts.Any())
            {
                await _db.TokenContracts.AddRangeAsync(newContracts, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }
        finally
        {
            await _appConfigService.ReleaseProcessLock(IMPORT_TOKEN_CONTRACT_PROCESSNAME, cancellationToken);
        }

        // Return te contracts
        return await _db.TokenContracts
            .AsNoTracking()
            .Where(c => guids.Contains(c.CryptoId))
            .Include(c => c.CryptoCurrency)
            .Include(c => c.BlockchainNetwork)
            .AsSingleQuery()
            .ToArrayAsync(cancellationToken);
    }

    #endregion
}
