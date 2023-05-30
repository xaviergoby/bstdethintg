using Microsoft.EntityFrameworkCore.Storage;

namespace Hodl.Api.Services;

public class FundService : IFundService
{
    private const string CLOSE_BOOKING_PERIOD_PROCESSNAME = "BookingPeriod.Closing";
    private const string DAILY_NAV_PROCESSNAME = "DailyNav.Calculating";

    private readonly string[] defaultCurrencyHoldings = new string[2] { "EUR", "USD" };
    private readonly string[] defaultCryptoHoldings = new string[4] {
        "fb3194c2-e732-4219-89b9-bc63eea2a861",
        "796923d5-a790-40d1-9074-c8fe3c25d049",
        "73f3f464-e442-4dcd-8b9c-7cf902931e89",
        "a5a7e6cb-ccdf-49ea-a4e8-6b6d6467004a"
        };

    private readonly HodlDbContext _db;
    private readonly IBookingPeriodHelper _bookingPeriodHelper;
    private readonly IAppConfigService _appConfigService;
    private readonly ICurrencyService _currencyService;
    private readonly ICryptoCurrencyService _cryptoService;
    private readonly ILayerIdxService _layerIdxService;
    private readonly IChangeLogService _changeLogService;
    private readonly IErrorManager _errorInformationManager;
    private readonly ILogger<FundService> _logger;

    private class HoldingSum
    {
        public decimal BTCValue { get; set; }
        public decimal USDValue { get; set; }
    }

    public FundService(
        HodlDbContext db,
        IBookingPeriodHelper bookingPeriodHelper,
        IAppConfigService appConfigService,
        ICurrencyService currencyService,
        ICryptoCurrencyService cryptoCurrencyService,
        ILayerIdxService layerIdxService,
        IChangeLogService changeLogService,
        IErrorManager errorInformationManager,
        ILogger<FundService> logger)
    {
        _db = db;
        _bookingPeriodHelper = bookingPeriodHelper;
        _appConfigService = appConfigService;
        _currencyService = currencyService;
        _cryptoService = cryptoCurrencyService;
        _layerIdxService = layerIdxService;
        _changeLogService = changeLogService;
        _errorInformationManager = errorInformationManager;
        _logger = logger;
    }

    public async Task<PagingModel<Fund>> GetFunds(int page, int? itemsPerPage, CancellationToken cancellationToken = default)
    {
        var query = _db.Funds
            .AsNoTracking()
            .Include(f => f.FundOwner)
            .OrderBy(c => c.FundName);

        // Create a paged resultset
        return await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, cancellationToken);
    }

    public async Task<Fund> GetFund(Guid fundId, CancellationToken cancellationToken = default) =>
        await _db.Funds.SingleOrDefaultAsync(f => f.Id.Equals(fundId), cancellationToken);

    public async Task<Fund> GetFundDetailed(Guid fundId, CancellationToken cancellationToken = default)
    {
        var currentBookingPeriod = await CurrentBookingPeriod(fundId, cancellationToken);

        var fund = await _db.Funds
            .AsNoTracking()
            .Where(f => f.Id == fundId)
            .Include(f => f.FundOwner)
            .Include(f => f.Layers.OrderBy(l => l.LayerIndex))
            .Include(f => f.FundCategories.OrderBy(fc => fc.Category.Group).ThenBy(fc => fc.Category.Name))
            .ThenInclude(fc => fc.Category)
            .Include(f => f.Holdings.Where(h => h.BookingPeriod == currentBookingPeriod && (h.StartBalance != 0 || h.NavBalance != 0 || h.EndBalance != 0)))
            .Include(f => f.DailyNavs
                .Where(nav => nav.Type == NavType.Daily && nav.BookingPeriod == currentBookingPeriod)
                .OrderByDescending(nav => nav.Date))
            .Include(f => f.BankAccounts)
            .ThenInclude(ba => ba.Bank)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);

        return fund;
    }

    public async Task AddFund(Fund fund)
    {
        // Check if unique condition is matched: nameof(Symbol), nameof(Name)
        if (await _db.Funds.AnyAsync(f => f.FundName == fund.FundName))
            _errorInformationManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Fund.FundName),
                Code = ErrorCodesStore.DuplicateKey,
                Description = "The fund name is already in use."
            });
        // Does the reporting currency exist?
        if (!await _db.Currencies.AnyAsync(c => c.ISOCode == fund.ReportingCurrencyCode))
            _errorInformationManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Fund.ReportingCurrencyCode),
                Code = ErrorCodesStore.ReferenceCheckFailed,
                Description = "The fund reporting currency does not exist."
            });
        // And the primary crypto curreny?
        if (!await _db.CryptoCurrencies.AnyAsync(c => c.Id == fund.PrimaryCryptoCurrencyId))
            _errorInformationManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Fund.PrimaryCryptoCurrencyId),
                Code = ErrorCodesStore.ReferenceCheckFailed,
                Description = "The fund primary crypto currency does not exist."
            });
        // Check the fund fee frequency
        if (fund.AdministrationFeeFrequency <= 0)
            _errorInformationManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Fund.AdministrationFeeFrequency),
                Code = ErrorCodesStore.EmptyValue,
                Description = "The fund administration fee frequency can not be 0 or smaller."
            });

        _errorInformationManager.ThrowOnErrors();


        // Check the start and end dates, can not be MinDate
        if (fund.DateStart == DateTime.MinValue)
        {
            fund.DateStart = _bookingPeriodHelper.GetPeriodStartDateTime(_bookingPeriodHelper.CalcBookingPeriod(DateTimeOffset.Now)).UtcDateTime;
        }
        if (fund.DateEnd == DateTime.MinValue || fund.DateEnd == DateTime.MaxValue)
        {
            fund.DateEnd = null;
        }
        if (string.IsNullOrWhiteSpace(fund.LayerStrategy))
        {
            fund.LayerStrategy = "default";
        }

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("Funds", null, fund);
            await _db.Funds.AddAsync(fund);
            await _db.SaveChangesAsync();

            if (string.Compare(fund.LayerStrategy, "default", true) == 0)
            {
                // Create default layer structure (3 layers)
                await _db.FundLayers.AddAsync(new FundLayer()
                {
                    FundId = fund.Id,
                    LayerIndex = 1,
                    Name = "Foundation Layer",
                    AimPercentage = 45,
                    AlertRangeHigh = 15,
                    AlertRangeLow = 5
                });
                await _db.FundLayers.AddAsync(new FundLayer()
                {
                    FundId = fund.Id,
                    LayerIndex = 2,
                    Name = "Floor Layer",
                    AimPercentage = 35,
                    AlertRangeHigh = 5,
                    AlertRangeLow = 5
                });
                await _db.FundLayers.AddAsync(new FundLayer()
                {
                    FundId = fund.Id,
                    LayerIndex = 3,
                    Name = "Top Layer",
                    AimPercentage = 20,
                    AlertRangeHigh = 10,
                    AlertRangeLow = 10
                });
                await _db.SaveChangesAsync();
            }

            // And create Holdings for the PrimaryCryptoCurrency and the reporting
            // currency and if they differ from BTC, ETH, BNB, USDT, EUR and USD add them too.
            List<Holding> holdings = new();
            var startDateOffset = new DateTimeOffset(fund.DateStart);
            var btcListing = await _cryptoService.GetListingByDate(_cryptoService.BtcGuid, startDateOffset, _cryptoService.PreferedListingSource, true);
            var bookingPeriod = _bookingPeriodHelper.CalcBookingPeriod(startDateOffset);
            foreach (var currencyCode in defaultCurrencyHoldings)
            {
                holdings.Add(await NewCurrencyHolding(fund, startDateOffset, btcListing, bookingPeriod, currencyCode));
            }
            if (!defaultCurrencyHoldings.Contains(fund.ReportingCurrencyCode))
            {
                holdings.Add(await NewCurrencyHolding(fund, startDateOffset, btcListing, bookingPeriod, fund.ReportingCurrencyCode));
            }
            foreach (var cryptoId in defaultCryptoHoldings)
            {
                holdings.Add(await NewCryptoHolding(fund, startDateOffset, bookingPeriod, new Guid(cryptoId)));
            }
            if (!defaultCryptoHoldings.Contains(fund.PrimaryCryptoCurrencyId.ToString()))
            {
                holdings.Add(await NewCryptoHolding(fund, startDateOffset, bookingPeriod, fund.PrimaryCryptoCurrencyId));
            }

            await _db.Holdings.AddRangeAsync(holdings);
            await _db.SaveChangesAsync();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }

        async Task<Holding> NewCurrencyHolding(Fund fund, DateTimeOffset startDate, Listing btcListing, string bookingPeriod, string currencyCode)
        {

            var rate = await _currencyService.GetCurrencyRatingByDate(currencyCode, startDate);
            var startUSDPrice = rate == null ? 0 : rate.USDRate;
            var startBTCPrice = rate == null || btcListing == null ? 0 : 1 / (btcListing.USDPrice / rate.USDRate);

            return new Holding
            {
                FundId = fund.Id,
                LayerIndex = 1,
                CurrencyISOCode = currencyCode,
                CurrencyRateId = rate?.Id,
                BookingPeriod = bookingPeriod,
                StartDateTime = fund.DateStart,
                StartBalance = 0,
                StartUSDPrice = startUSDPrice,
                StartBTCPrice = startBTCPrice,
                EndBalance = 0,
                EndUSDPrice = startUSDPrice,
                EndBTCPrice = startBTCPrice
            };
        }
        async Task<Holding> NewCryptoHolding(Fund fund, DateTimeOffset startDate, string bookingPeriod, Guid cryptoId)
        {
            var listing = await _cryptoService.GetListingByDate(cryptoId, startDate, _cryptoService.PreferedListingSource, true);

            return new Holding
            {
                FundId = fund.Id,
                LayerIndex = 1,
                CryptoId = cryptoId,
                ListingId = listing?.Id,
                BookingPeriod = bookingPeriod,
                StartDateTime = fund.DateStart,
                StartBalance = 0,
                StartUSDPrice = listing?.USDPrice ?? 0,
                StartBTCPrice = listing?.BTCPrice ?? 0,
                EndBalance = 0,
                EndUSDPrice = listing?.USDPrice ?? 0,
                EndBTCPrice = listing?.BTCPrice ?? 0
            };
        }
    }

    public async Task UpdateFund(Fund fund)
    {
        var storedFund = await _db.Funds
            .AsNoTracking()
            .Where(f => f.Id == fund.Id)
            .SingleOrDefaultAsync() ??
            throw new NotFoundException($"Fund with fundId {fund.Id} not found.");

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("Funds", storedFund, fund);

            // Store the first bookingperiod for the fund to check changes
            var oldBookingPeriod = _bookingPeriodHelper.CalcBookingPeriod(new DateTimeOffset(storedFund.DateStart));
            var newBookingPeriod = _bookingPeriodHelper.CalcBookingPeriod(new DateTimeOffset(fund.DateStart));

            _db.Funds.Update(fund);
            await _db.SaveChangesAsync();

            // If the startdate/first bookingperiod is changed, check the holdings
            // and modify the period there too, but only if the only bookingperiod
            // is the first period on the fund.
            if (oldBookingPeriod != newBookingPeriod &&
                await CurrentBookingPeriod(fund.Id) == oldBookingPeriod)
            {
                var holdings = await _db.Holdings
                    .Where(h => h.FundId == fund.Id)
                    .ToArrayAsync();

                foreach (var holding in holdings)
                {
                    holding.BookingPeriod = newBookingPeriod;
                    holding.StartDateTime = storedFund.DateStart;
                }

                await _db.SaveChangesAsync();
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }
    }

    public async Task DeleteFund(Guid fundId)
    {
        var storedFund = await _db.Funds
            .Where(c => c.Id == fundId)
            .SingleOrDefaultAsync() ??
            throw new NotFoundException($"Fund with FundId {fundId} not found.");

        // checks for existing holdings or NAV's and return nice error messages.
        if (await _db.Holdings.AnyAsync(h => h.Id == fundId && h.EndBalance != 0))
            _errorInformationManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Fund.Id),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "The fund has holdings and so can not be deleted."
            });
        if (await _db.Navs.AnyAsync(dn => dn.Id == fundId))
            _errorInformationManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Fund.DailyNavs),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "The fund has NAV's and so can not be deleted."
            });

        _errorInformationManager.ThrowOnErrors();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("Funds", storedFund, null);
            var holdings = await _db.Holdings
                .Where(h => h.FundId == fundId && h.EndBalance == 0)
                .ToArrayAsync();
            var navs = await _db.Navs
                .Where(h => h.FundId == fundId && h.Type == NavType.Daily)
                .ToArrayAsync();
            _db.Holdings.RemoveRange(holdings);
            _db.Navs.RemoveRange(navs);
            _db.Funds.Remove(storedFund);
            await _db.SaveChangesAsync();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }
    }

    public async Task<string[]> GetAllBookingPeriods(Guid fundId, CancellationToken cancellationToken = default) =>
        await _db.Holdings
            .AsNoTracking()
            .Where(h => h.FundId == fundId)
            .Select(h => h.BookingPeriod)
            .Distinct()
            .OrderByDescending(i => i)
            .ToArrayAsync(cancellationToken);

    public async Task<PagingModel<FundOwner>> GetFundOwners(int page, int? itemsPerPage, CancellationToken cancellationToken = default)
    {
        var query = _db.FundOwners
            .AsNoTracking()
            .OrderBy(c => c.Name);

        // Create a paged resultset
        return await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, cancellationToken);
    }

    /// <summary>
    /// Returns all funds that are active. A fund is active when the startdate
    /// is in the past and the close date is not set or in the future or when
    /// there are still holdings with an open bookingperiod (not closed).
    /// </summary>
    /// <returns></returns>
    public async Task<IList<Fund>> GetActiveFunds(CancellationToken cancellationToken = default)
    {
        var openHoldingFundIds = await _db.Holdings
            .Where(h => !h.PeriodClosedDateTime.HasValue)
            .Select(h => h.FundId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _db.Funds
            .Where(f => f.DateStart <= DateTime.UtcNow && (f.DateEnd == null || f.DateEnd >= DateTime.UtcNow || openHoldingFundIds.Contains(f.Id)))
            .OrderBy(f => f)
            .ToListAsync(cancellationToken);
    }

    public async Task<IList<Fund>> GetActiveFundsSortedByDependency(CancellationToken cancellationToken = default)
    {
        var openHoldings = await _db.Holdings
            .Where(h => !h.PeriodClosedDateTime.HasValue)
            .ToListAsync(cancellationToken);

        var fundIds = openHoldings
            .Select(h => h.FundId)
            .Distinct();

        var funds = await _db.Funds
            .Where(f => f.DateStart <= DateTime.UtcNow && (f.DateEnd == null || f.DateEnd >= DateTime.UtcNow || fundIds.Contains(f.Id)))
            .ToListAsync(cancellationToken);

        var dependencies = openHoldings
            .Where(h => h.SharesFundId != null)
            .Select(h => (funds.Single(f => f.Id == h.FundId), funds.Single(f => f.Id == h.SharesFundId)));

        return DependencySort.Sort(funds, dependencies);
    }

    public async Task<IEnumerable<Fund>> GetFundCards(CancellationToken cancellationToken = default)
    {
        var activeFunds = await GetActiveFunds(cancellationToken);

        return await _db.Funds
            .AsNoTracking()
            .Where(f => activeFunds.Contains(f))
            .Include(f => f.FundOwner)
            .Include(f => f.Layers.OrderBy(l => l.LayerIndex))
            .Include(f => f.DailyNavs
                .Where(nav => nav.Type == NavType.Daily)
                .OrderByDescending(nav => nav.Date)
                .Take(50))
            .ThenInclude(n => n.CurrencyRate)
            .OrderBy(c => c.FundName)
            .AsSplitQuery()
            .ToArrayAsync(cancellationToken);
    }

    public async Task<string> CurrentBookingPeriod(Guid fundId, CancellationToken cancellationToken = default)
    {
        var bookingPeriod = await _db.Holdings
            .Where(x => x.FundId == fundId && x.PeriodClosedDateTime == null)
            .MaxAsync(x => x.BookingPeriod, cancellationToken);

        return bookingPeriod ?? _bookingPeriodHelper.CalcBookingPeriod(DateTimeOffset.UtcNow);
    }

    public async Task<string> GetValidBookingPeriod(string bookingPeriod, Guid fundId, CancellationToken cancellationToken = default)
    {
        string currentBookingPeriod = await CurrentBookingPeriod(fundId, cancellationToken);

        return string.IsNullOrEmpty(bookingPeriod)
            || bookingPeriod.Length != 6
            || !bookingPeriod.All(char.IsDigit)
            || string.Compare(bookingPeriod, currentBookingPeriod) > 0
            ? currentBookingPeriod
            : bookingPeriod;
    }


    /// <summary>
    /// Gets the holdings for the current (latest) booking period for the selected fund.
    /// </summary>
    /// <param name="fundId"></param>
    /// <returns></returns>
    public async Task<IList<Holding>> GetCurrentFundHoldings(Guid fundId, bool filterUnused = true, bool setLatestListings = true, CancellationToken cancellationToken = default)
    {
        var bookingPeriod = await CurrentBookingPeriod(fundId, cancellationToken);

        var holdings = await GetFundHoldings(fundId, bookingPeriod, bookingPeriod, filterUnused, cancellationToken);

        if (setLatestListings)
        {
            // Get all listings and currency ratings to set to the holdings
            var btcListing = await _cryptoService.GetMostRecentListing(_cryptoService.BtcGuid, _cryptoService.PreferedListingSource, cancellationToken);

            await SetEndPrices(holdings, btcListing, cancellationToken);
        }

        return holdings;
    }

    public async Task<IList<Holding>> GetFundHoldings(Guid fundId, string fromBookingPeriod, string toBookingPeriod, bool filterUnused = true, CancellationToken cancellationToken = default)
    {
        fromBookingPeriod = string.IsNullOrEmpty(fromBookingPeriod) ? "000000" : fromBookingPeriod;
        toBookingPeriod = string.IsNullOrEmpty(toBookingPeriod) ? await CurrentBookingPeriod(fundId, cancellationToken) : toBookingPeriod;

        var holdings = await _db.Holdings
            .AsNoTracking()
            .Where(h => h.FundId == fundId
                && h.BookingPeriod.CompareTo(fromBookingPeriod) >= 0
                && h.BookingPeriod.CompareTo(toBookingPeriod) <= 0
                && (!filterUnused || h.StartBalance != 0 || h.NavBalance != 0 || h.EndBalance != 0))
            .Include(h => h.Currency)
            .Include(h => h.CryptoCurrency)
            .Include(h => h.SharesFund)
            .OrderByDescending(h => h.BookingPeriod)
            .ThenBy(h => h.Currency.Name ?? h.CryptoCurrency.Name)
            .AsSingleQuery()
            .ToListAsync(cancellationToken);

        return holdings;
    }

    public async Task<Holding> AddHolding(Holding holding, CancellationToken cancellationToken = default)
    {
        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _changeLogService.AddChangeLogAsync("Holdings", null, holding, cancellationToken);

            // And create a listing when there is none available
            if (holding.CryptoId != null)
            {
                //var listing = !await _db.Listings.AnyAsync(l => l.CryptoId == newHolding.CryptoId)
                var listing = await _cryptoService.GetMostRecentListing((Guid)holding.CryptoId, _cryptoService.PreferedListingSource, cancellationToken);

                if (listing == null)
                {
                    listing = new Listing()
                    {
                        CryptoId = (Guid)holding.CryptoId,
                        Source = "Manual",
                        TimeStamp = holding.StartDateTime,
                        USDPrice = holding.StartUSDPrice,
                        BTCPrice = holding.StartBTCPrice
                    };

                    await _db.Listings.AddAsync(listing, cancellationToken);
                    await _db.SaveChangesAsync(cancellationToken);
                }

                holding.ListingId = listing.Id;
            }

            await _db.Holdings.AddAsync(holding, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // Recalc start and end percentages
            _ = await RecalcPercentages(holding.FundId, holding.BookingPeriod, cancellationToken);

            // And trigger background process for the listing history retrieval in case of a crypto holding.
            if (holding.CryptoId != null)
            {
                var crypto = await _cryptoService.GetCryptoCurrencyAsync((Guid)holding.CryptoId, cancellationToken);
                await _cryptoService.TriggerListingsHistory(crypto.Symbol, cancellationToken);
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }

        return holding;
    }

    public async Task<Nav> GetCurrentNav(Guid fundId, CancellationToken cancellationToken = default) =>
        await _db.Navs
            .Where(nav => nav.FundId == fundId && nav.Type == NavType.Period)
            .Include(nav => nav.CurrencyRate)
            .OrderByDescending(nav => nav.BookingPeriod)
            .ThenByDescending(r => r.DateTime)
            .AsSingleQuery()
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Nav> GetNavByDate(Guid fundId, DateTimeOffset datetime, CancellationToken cancellationToken = default) =>
        await _db.Navs
            .Where(n => n.FundId.Equals(fundId) && n.Type.Equals(NavType.Period) && n.Date <= datetime.UtcDateTime)
            .Include(nav => nav.CurrencyRate)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.DateTime)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<string[]> GetFundGategoryGroups(Guid fundId, CancellationToken cancellationToken = default)
    {
        return await _db.FundCategories
            .Where(fc => fc.FundId == fundId)
            .Include(fc => fc.Category)
            .Select(fc => fc.Category.Group)
            .Distinct()
            .OrderBy(i => i)
            .AsSingleQuery()
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Holding> GetOrCreateFundHolding(Guid fundId, Guid? cryptoCurrencyId, string currencyISOCode = null, string bookingPeriod = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(bookingPeriod))
            bookingPeriod = await CurrentBookingPeriod(fundId, cancellationToken);

        var holding = await _db.Holdings
            .FirstOrDefaultAsync(h => h.FundId == fundId
                && h.BookingPeriod == bookingPeriod
                && ((cryptoCurrencyId != null && h.CryptoId == cryptoCurrencyId)
                || (!string.IsNullOrEmpty(currencyISOCode) && h.CurrencyISOCode == currencyISOCode)),
                cancellationToken);

        return holding ?? await CreateHolding(fundId, cryptoCurrencyId, currencyISOCode, bookingPeriod, cancellationToken);
    }

    /// <summary>
    /// Closing the booking period takes care of creating and calculating all
    /// the holdings in the funds.
    /// All the transfers (except in- and outflow) and trades registered in the
    /// booking period and are added and/or subtracted so the end-balance can
    /// be calculated. Then the NAV is calculated and stored. As last step in
    /// the process, the in- and outflow is processed. The shares are
    /// calculated from the NAV and the balances of the holdings and the shares
    /// are added and subtracted.
    /// The bookingperiod is closed by setting the PeriodClosedDateTime on all
    /// the holdings. New holdings will be created for the new booking period.
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="bookingPeriod"></param>
    /// <param name="forceRecalculation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="BookingPeriodClosedException"></exception>
    /// <exception cref="RestException"></exception>
    public async Task<string> CloseBookingPeriod(Guid fundId, string bookingPeriod, bool forceRecalculation = false, CancellationToken cancellationToken = default)
    {
        string activePeriod = _bookingPeriodHelper.CalcBookingPeriod(DateTimeOffset.Now);
        string currentPeriod = await CurrentBookingPeriod(fundId, cancellationToken);
        string closeBookingPeriod = await GetValidBookingPeriod(bookingPeriod, fundId, cancellationToken);
        string nextBookingPeriod = _bookingPeriodHelper.GetNextBookingPeriod(closeBookingPeriod);
        var periodStart = _bookingPeriodHelper.GetPeriodStartDateTime(closeBookingPeriod);
        var periodEnd = _bookingPeriodHelper.GetPeriodEndDateTime(closeBookingPeriod);

        var fund = await _db.Funds.SingleAsync(f => f.Id == fundId, cancellationToken) ??
            throw new BookingPeriodClosedException($"Fund not found: {fundId}.");

        if (fund.DateEnd != null && fund.DateEnd < periodStart.UtcDateTime)
            throw new BookingPeriodClosedException($"Fund is closed at {fund.DateEnd} so bookingperiod {closeBookingPeriod} does not exist.");

        if (string.Compare(closeBookingPeriod, activePeriod) >= 0)
            throw new BookingPeriodClosedException($"Can not close booking period {closeBookingPeriod}. Booking period is not finished or in the future.");

        if (closeBookingPeriod != currentPeriod && !forceRecalculation)
            throw new BookingPeriodClosedException($"Can not close booking period {closeBookingPeriod}. Booking period is already closed.");

        if (!await _appConfigService.RequestProcessLock(CLOSE_BOOKING_PERIOD_PROCESSNAME, 5 * 60, cancellationToken))
            throw new RestException(HttpStatusCode.Conflict, $"An other process is closing bookingperiods. Only one process can be run at a time.");

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (closeBookingPeriod != currentPeriod)
            {
                _logger.LogInformation("Recalculate booking period {period} for fundid {fund}.", closeBookingPeriod, fundId);
            }
            else
            {
                _logger.LogInformation("Close booking period {period} for fundid {fund}.", closeBookingPeriod, fundId);
            }

            IList<Holding> holdings = await CalcPeriodEndBalances(fundId, closeBookingPeriod, periodEnd, cancellationToken);

            // Sum the holding BTC and USD values and create holdings for the
            // new bookingperiod
            var holdingsSum = await CalcEndValues(holdings, periodEnd, cancellationToken);
            // Loop to set the end percentage and the NAVBalance
            foreach (var holding in holdings)
            {
                holding.NavBalance = holding.EndBalance;
                holding.EndPercentage = holding.EndBalance > 0
                    ? (holding.EndUSDPrice * holding.EndBalance) / holdingsSum.USDValue * 100
                    : 0;
            }
            // Then create the period NAV
            var navDate = _bookingPeriodHelper.NavDate(periodEnd);
            var nav = await CreateNav(fund, holdingsSum.USDValue, NavType.Period, navDate, closeBookingPeriod, cancellationToken);

            // Process the in- and outflow. Calculate the shares for the
            // transfers and add the values to the totalshares and holding
            // balances. This is only done on closing the booking period.
            Transfer[] transfers = await GetAllHoldingTransfers(holdings, cancellationToken);
            foreach (var transfer in transfers)
            {
                if (transfer.TransactionType == TransactionType.Outflow ||
                    transfer.TransactionType == TransactionType.Inflow)
                {
                    var holding = holdings.Single(h => h.Id == transfer.HoldingId);

                    // Set the bookingperiod
                    transfer.BookingPeriod = closeBookingPeriod;

                    // Calculate the shares when not yet set, or when forced recalculation is required.
                    if (transfer.Shares == 0 || (forceRecalculation && transfer.TransferAmount != 0))
                    {
                        transfer.Shares = await CalcShares(holding, transfer, nav, cancellationToken);
                    }
                    // When outflow is given in number of shares and the amount is not set,
                    // calculate the value of the shares and set the ammount.
                    if (transfer.TransactionType == TransactionType.Outflow && transfer.TransferAmount == 0)
                    {
                        transfer.TransferAmount = await CalcOutflowAmount(holding, transfer, nav, cancellationToken);
                    }

                    // Now, if there is a referencing transfer (created by transfering
                    // holdings from an other fund), then the number of shares should
                    // be added to the shares holding.
                    if (transfer.OppositeTransferId != null)
                    {
                        var refTransfer = await _db.Transfers.SingleAsync(t => t.Id == transfer.OppositeTransferId, cancellationToken);
                        var valueTransfer = await _db.Transfers.SingleAsync(t => t.Id == refTransfer.OppositeTransferId, cancellationToken);

                        // With inflow, the asset is transfered and the number of shares must be filled
                        // in the value transfer. This is the number of shares in the shares holding.
                        // When the transfer is outflow, the asset amount must be returned in the holding.
                        valueTransfer.TransferAmount = transfer.TransactionType == TransactionType.Inflow
                            ? transfer.Shares
                            : transfer.TransferAmount;
                    }

                    // And finally calculate the end balance
                    if (transfer.TransactionType == TransactionType.Outflow)
                    {
                        // Decrease the shares on the fund.
                        holding.EndBalance -= transfer.TransferAmount;
                        nav.InOutValue -= (transfer.TransferAmount * holding.EndUSDPrice) / nav.CurrencyRate.USDRate;
                        nav.InOutShares -= transfer.Shares;
                    }
                    if (transfer.TransactionType == TransactionType.Inflow)
                    {
                        // Increase the shares on the fund.
                        holding.EndBalance += transfer.TransferAmount;
                        nav.InOutValue += (transfer.TransferAmount * holding.EndUSDPrice) / nav.CurrencyRate.USDRate;
                        nav.InOutShares += transfer.Shares;
                    }
                }
            }
            await _db.Navs.AddAsync(nav, cancellationToken);

            // And loop ones more to set the closedatetime and create the
            // follow-up holdings for the new period. The actual closing.
            DateTimeOffset closeDateTime = DateTimeOffset.Now;
            foreach (var holding in holdings)
            {
                holding.EndDateTime = periodEnd.UtcDateTime;
                holding.PeriodClosedDateTime = closeDateTime.UtcDateTime;

                // And create the holding for the new period
                if (holding.EndBalance != 0 && (fund.DateEnd == null || fund.DateEnd > periodEnd.UtcDateTime))
                {
                    // Every bookingperiod holdings are created for all assets in the fund.
                    // The holding for the new bokingperiod starts with the end-balance of
                    // the presious holding.
                    var nextHolding = await GetOrCreateFundHolding(holding.FundId, holding.CryptoId, holding.CurrencyISOCode, nextBookingPeriod, cancellationToken);
                    nextHolding.PreviousHoldingId = holding.Id;
                    nextHolding.LayerIndex = await _layerIdxService.IdxAssignmentStrategy(nextHolding, cancellationToken);
                    nextHolding.StartDateTime = periodEnd.UtcDateTime;
                    nextHolding.StartBalance = holding.EndBalance;
                    nextHolding.StartUSDPrice = holding.EndUSDPrice;
                    nextHolding.StartBTCPrice = holding.EndBTCPrice;
                    nextHolding.StartPercentage = holding.EndPercentage;
                    nextHolding.CurrencyRateId = holding.CurrencyRateId;
                    nextHolding.ListingId = holding.ListingId;
                    nextHolding.EndBalance = holding.EndBalance;
                    nextHolding.EndUSDPrice = holding.EndUSDPrice;
                    nextHolding.EndBTCPrice = holding.EndBTCPrice;
                }
            }

            // Add the totals to the fund if the closed bookingperiod is the
            // last active period or the next period is the active period (in
            // case of recalculations)
            if (currentPeriod == closeBookingPeriod ||
                currentPeriod == nextBookingPeriod)
            {
                var reportinCurrencyRate = await _currencyService.GetCurrencyRatingByDate(fund.ReportingCurrencyCode, periodEnd, cancellationToken);
                fund.TotalValue = nav.TotalValue + nav.InOutValue;
                fund.TotalShares = nav.TotalShares + nav.InOutShares;
            }
            // HWM is always updated, since it is the highest share value, even
            // when a recalculation is done.
            fund.ShareValueHWM = Math.Max(nav.ShareNAV, fund.ShareValueHWM);

            await _db.SaveChangesAsync(cancellationToken);
            await _changeLogService.AddChangeLogAsync("CloseBookingPeriod", bookingPeriod, nav, cancellationToken);
            _logger.LogInformation("Booking period {period} for fundid {fund} is closed.", closeBookingPeriod, fundId);

            // And when all is done, recalc the end balances for the new period
            var nextPeriodEnd = _bookingPeriodHelper.GetPeriodEndDateTime(closeBookingPeriod);
            IList<Holding> nextHoldings = await CalcPeriodEndBalances(fundId, nextBookingPeriod, nextPeriodEnd, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            transaction.Commit();
        }
        catch (Exception e)
        {
            _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, DateTime.Now, e.Message);
            transaction.Rollback();

            // Reraise the error
            throw;
        }
        finally
        {
            await _appConfigService.ReleaseProcessLock(CLOSE_BOOKING_PERIOD_PROCESSNAME, cancellationToken);
        }

        return nextBookingPeriod;
    }

    public async Task<string> CloseAllBookingPeriods(Guid fundId, CancellationToken cancellationToken = default)
    {
        string lastPeriod = await CurrentBookingPeriod(fundId, cancellationToken);
        string currentPeriod = _bookingPeriodHelper.CalcBookingPeriod(DateTimeOffset.Now);

        while (string.Compare(lastPeriod, currentPeriod) < 0 && !cancellationToken.IsCancellationRequested)
        {
            lastPeriod = await CloseBookingPeriod(fundId, lastPeriod, cancellationToken: cancellationToken);
        }

        return lastPeriod;
    }

    public async Task<string> RollbackCloseBookingPeriod(Guid fundId, CancellationToken cancellationToken = default)
    {
        string activePeriod = await CurrentBookingPeriod(fundId, cancellationToken);
        string previousPeriod = _bookingPeriodHelper.GetPreviousBookingPeriod(activePeriod);

        var fund = await _db.Funds
            .SingleAsync(f => f.Id == fundId, cancellationToken);
        var activeHoldings = await _db.Holdings
            .Where(h => h.FundId == fundId && h.BookingPeriod == activePeriod)
            .ToArrayAsync(cancellationToken);

        if (fund == null)
            throw new BookingPeriodClosedException($"Fund not found: {fundId}.");

        // Check if no transfers are linked to the latest holdings. That prevents deleting the holdings.
        if (await _db.Transfers.AnyAsync(t => activeHoldings.Select(h => h.Id).Contains(t.HoldingId), cancellationToken))
            throw new BookingPeriodClosedException($"Can not rollback booking period closing for {previousPeriod}. The new booking period has transfers registered to the latest holdings.");

        if (!await _appConfigService.RequestProcessLock(CLOSE_BOOKING_PERIOD_PROCESSNAME, 5 * 60, cancellationToken))
            throw new RestException(HttpStatusCode.Conflict, $"An other process is closing bookingperiods. Only one process can be run at a time.");

        // Start transaction
        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Rollback booking period closing of period {period} for fundid {fund}.", previousPeriod, fundId);

            var navs = await _db.Navs
                .Where(n => n.FundId == fundId && n.BookingPeriod == previousPeriod)
                .ToArrayAsync(cancellationToken);
            var previousHoldings = await _db.Holdings
                .Where(h => h.FundId == fundId && h.BookingPeriod == previousPeriod)
                .ToArrayAsync(cancellationToken);
            var previousTransfers = await _db.Transfers
                .Where(t => previousHoldings.Select(h => h.Id).Contains(t.HoldingId))
                .ToArrayAsync(cancellationToken);
            var trades = await _db.Trades
                .Where(t => t.BookingPeriod == previousPeriod)
                .ToArrayAsync(cancellationToken);

            _db.Navs.RemoveRange(navs);
            _db.Holdings.RemoveRange(activeHoldings);

            foreach (var transfer in previousTransfers)
            {
                if (transfer.TransactionType == TransactionType.Outflow ||
                    transfer.TransactionType == TransactionType.Inflow)
                {
                    var holding = previousHoldings.Single(h => h.Id == transfer.HoldingId);

                    if (transfer.TransactionType == TransactionType.Outflow)
                    {
                        // Revert the decrease the shares on the fund.
                        holding.EndBalance += transfer.TransferAmount;
                    }
                    if (transfer.TransactionType == TransactionType.Inflow)
                    {
                        // Revert the increase the shares on the fund.
                        holding.EndBalance -= transfer.TransferAmount;
                    }

                    // And reset the shares
                    transfer.Shares = 0;
                }
            }
            foreach (var holding in previousHoldings)
            {
                holding.PeriodClosedDateTime = null;
                holding.NavBalance = 0;
                holding.CurrencyRateId = null;
                holding.ListingId = null;
                holding.EndDateTime = null;
            }
            foreach (var trade in trades)
            {
                trade.BookingPeriod = string.Empty;
            }

            // And find the previous NAV to reset the fund totals and HWM
            var previousNav = await _db.Navs
                .OrderByDescending(nav => nav.DateTime)
                .FirstOrDefaultAsync(nav => nav.FundId == fundId
                    && nav.Type == NavType.Period
                    && nav.BookingPeriod == _bookingPeriodHelper.GetPreviousBookingPeriod(previousPeriod),
                    cancellationToken);

            fund.ShareValueHWM = previousNav?.ShareHWM ?? 1;
            fund.TotalValue = previousNav?.TotalValue + previousNav?.InOutValue ?? 0;
            fund.TotalShares = previousNav?.TotalShares + previousNav?.InOutShares ?? 0;

            await _db.SaveChangesAsync(cancellationToken);

            await _changeLogService.AddChangeLogAsync("RollbackCloseBookingPeriod", previousPeriod, previousNav, cancellationToken);

            _logger.LogInformation("Booking period closing of period {period} for fundid {fundId} is rolled back.", activePeriod, fundId);

            transaction.Commit();
        }
        catch (Exception e)
        {
            _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, e.Message);
            transaction.Rollback();

            // Reraise the error
            throw;
        }
        finally
        {
            await _appConfigService.ReleaseProcessLock(CLOSE_BOOKING_PERIOD_PROCESSNAME, cancellationToken);
        }

        return previousPeriod;
    }

    public async Task<Nav> GetLastNAVFor(Guid fundId, NavType navType, CancellationToken cancellationToken = default)
    {
        return await _db.Navs
            .Include(nav => nav.CurrencyRate)
            .OrderByDescending(nav => nav.DateTime)
            .FirstOrDefaultAsync(nav => nav.FundId == fundId && nav.Type == navType, cancellationToken);
    }

    public async Task<Nav> CreateDailyNAV(Guid fundId, DateTime date, CancellationToken cancellationToken = default)
    {
        var periodEnd = _bookingPeriodHelper.DailyNavEndDateTime(date);
        string bookingPeriod = _bookingPeriodHelper.CalcBookingPeriod(periodEnd.AddMinutes(-1));
        var periodStart = _bookingPeriodHelper.GetPeriodStartDateTime(bookingPeriod);

        // Check if the fund has holdings in the selected booking period
        if (!await _db.Holdings.AnyAsync(h => h.FundId == fundId && h.BookingPeriod == bookingPeriod, cancellationToken))
            throw new RestException(HttpStatusCode.Conflict, $"Can not calculate NAV where no holdings can be found.");

        if (!await _appConfigService.RequestProcessLock(DAILY_NAV_PROCESSNAME, 5 * 60, cancellationToken))
            throw new RestException(HttpStatusCode.Conflict, $"An other process is calculating daily NAV's. Only one process can be run at a time.");

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Calculating daily NAV for {date:d} for fundid {fund}.", date, fundId);

            // Calculate the end balances for the holdings
            IList<Holding> holdings = await CalcPeriodEndBalances(fundId, bookingPeriod, periodEnd, cancellationToken);

            // Sum the holding BTC and USD values and create holdings for the
            // new bookingperiod
            var holdingsSum = await CalcEndValues(holdings, periodEnd, cancellationToken);

            // If the NAV date is the actual date, we can save the latest
            // balances and currency rates. If not, we have to prevent saving
            // the changes in the holdings.
            if (!date.Equals(DateTime.Today))
            {
                // Prevent the changes from being saved
                _db.RevertChanges(holdings);
            }

            // Create NAV
            var fund = await _db.Funds.SingleAsync(f => f.Id == fundId, cancellationToken);
            var navDate = _bookingPeriodHelper.NavDate(periodEnd);
            var nav = await CreateNav(fund, holdingsSum.USDValue, NavType.Daily, navDate, bookingPeriod, cancellationToken);

            await _db.Navs.AddAsync(nav, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Daily NAV for {date:d} is created.", navDate);

            transaction.Commit();

            return nav;
        }
        catch (Exception e)
        {
            _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, DateTime.Now, e.Message);
            transaction.Rollback();

            // Reraise the error
            throw;
        }
        finally
        {
            await _appConfigService.ReleaseProcessLock(DAILY_NAV_PROCESSNAME, cancellationToken);
        }
    }

    public async Task<IList<Holding>> RecalcEndBalances(Guid fundId, string bookingPeriod, CancellationToken cancellationToken = default)
    {
        var periodEnd = _bookingPeriodHelper.GetPeriodEndDateTime(bookingPeriod);

        var holdings = await CalcPeriodEndBalances(fundId, bookingPeriod, periodEnd, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);

        return holdings;
    }

    public async Task<IList<Holding>> RecalcPercentages(Guid fundId, string bookingPeriod, CancellationToken cancellationToken = default)
    {
        var holdings = await _db.Holdings
            .Where(h => h.FundId == fundId && h.BookingPeriod == bookingPeriod)
            .ToListAsync(cancellationToken);

        decimal sumStart = 0;
        decimal sumEnd = 0;
        foreach (var h in holdings)
        {
            sumStart += h.StartBalance * h.StartUSDPrice;
            sumEnd += h.EndBalance * h.EndUSDPrice;
        }

        foreach (var h in holdings)
        {
            h.StartPercentage = sumStart != 0
                ? (h.StartBalance * h.StartUSDPrice) / sumStart * 100
                : 0;
            h.EndPercentage = sumEnd != 0
                ? (h.EndBalance * h.EndUSDPrice) / sumEnd * 100
                : 0;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return holdings;
    }

    public async Task<IList<Holding>> CalcHoldingDistribution(ICollection<Holding> holdings, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var result = new List<Holding>();

            // Loop to get accurate value for the holdings and sums.
            decimal totalUsdValue = 0;
            foreach (var holding in holdings)
            {
                totalUsdValue += holding.EndBalance * holding.EndUSDPrice;

                result.Add(holding);
            }

            if (totalUsdValue > 0)
            {
                foreach (var holding in result)
                {
                    holding.EndPercentage = (holding.EndBalance * holding.EndUSDPrice / totalUsdValue) * 100;
                }
            }

            return result;
        });
    }

    /// <summary>
    /// Add all latest listings for the holdings.
    /// </summary>
    /// <param name="holdings"></param>
    /// <param name="btcListing"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task SetEndPrices(IEnumerable<Holding> holdings, Listing btcListing, CancellationToken cancellationToken)
    {
        var cryptos = holdings
            .Where(h => h.CryptoCurrency != null)
            .Select(h => h.CryptoCurrency.ListingCryptoId ?? h.CryptoCurrency.Id);

        foreach (var holding in holdings)
        {
            if (holding.CryptoId != null && holding.ListingId == null)
            {
                holding.Listing = await _cryptoService.GetMostRecentListing((Guid)holding.CryptoId, _cryptoService.PreferedListingSource, cancellationToken);
                holding.EndUSDPrice = holding.Listing?.USDPrice ?? holding.StartUSDPrice;
                holding.EndBTCPrice = holding.Listing?.BTCPrice ?? holding.StartBTCPrice;
            }
            else if (!string.IsNullOrEmpty(holding.CurrencyISOCode) && holding.CurrencyRateId == null)
            {
                holding.CurrencyRate = await _currencyService.GetMostRecentRating(holding.CurrencyISOCode, cancellationToken);
                holding.EndUSDPrice = holding.CurrencyRate.USDRate;
                holding.EndBTCPrice = 1 / (btcListing.USDPrice / holding.CurrencyRate.USDRate);
            }
        }
    }

    public async Task<IDictionary<byte, SumRecord>> CalcLayerDistribution(ICollection<Holding> holdings, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            Dictionary<byte, SumRecord> layerHoldings = new();
            decimal totalUsdValue = 0;
            foreach (var holding in holdings)
            {
                if (holding.LayerIndex <= 0) continue;

                if (!layerHoldings.ContainsKey(holding.LayerIndex))
                {
                    layerHoldings[holding.LayerIndex] = new SumRecord();
                }

                var USDValue = holding.EndBalance * holding.EndUSDPrice;
                var BTCValue = holding.EndBalance * holding.EndBTCPrice;
                var rec = layerHoldings[holding.LayerIndex];
                rec.USDValue += USDValue;
                rec.BTCValue += BTCValue;
                rec.NumberOfItems++;
                totalUsdValue += USDValue;
            }

            if (totalUsdValue > 0)
            {
                foreach (var layer in layerHoldings.Values)
                {
                    layer.TotalSharePercentage = (layer.USDValue / totalUsdValue) * 100;
                }
            }

            return layerHoldings;
        }, cancellationToken);
    }

    public async Task<IDictionary<Guid, SumRecord>> CalcCategoryDistribution(ICollection<FundCategory> fundCategories, ICollection<Holding> holdings, CancellationToken cancellationToken = default)
    {
        var categories = fundCategories.Select(fc => fc.CategoryId).ToArray();
        var categoryCryptos = await _db.CryptoCategories
            .Where(cc => categories.Contains(cc.CategoryId))
            .ToArrayAsync(cancellationToken);

        return await Task.Run(() =>
        {
            Dictionary<Guid, SumRecord> categoryDistribution = new();
            decimal totalUsdValue = 0;
            foreach (var categoryCrypto in categoryCryptos)
            {
                foreach (var holding in holdings.Where(h => h.CryptoId == categoryCrypto.CryptoId))
                {
                    if (!categoryDistribution.ContainsKey(categoryCrypto.CategoryId))
                    {
                        categoryDistribution[categoryCrypto.CategoryId] = new SumRecord();
                    }

                    var usdValue = holding.EndBalance * holding.EndUSDPrice;
                    var btcValue = holding.EndBalance * holding.EndBTCPrice;
                    var rec = categoryDistribution[categoryCrypto.CategoryId];
                    rec.USDValue += usdValue;
                    rec.BTCValue += btcValue;
                    rec.NumberOfItems++;
                    totalUsdValue += usdValue;
                }
            }

            // Count the fiat currencies only ones!!
            var fiatCategory = fundCategories.Where(c => c.Category.IncludeFiat).FirstOrDefault();
            if (fiatCategory != null)
            {
                decimal usdValue = 0;
                decimal btcValue = 0;
                int numberOfItems = 0;
                foreach (var holding in holdings.Where(h => !string.IsNullOrEmpty(h.CurrencyISOCode)))
                {
                    usdValue += holding.EndBalance * holding.EndUSDPrice;
                    btcValue += holding.EndBalance * holding.EndBTCPrice;
                    numberOfItems++;
                }

                var rec = categoryDistribution[fiatCategory.CategoryId];
                rec.USDValue += usdValue;
                rec.BTCValue += btcValue;
                rec.NumberOfItems += numberOfItems;
                totalUsdValue += usdValue;
            }

            if (totalUsdValue > 0)
            {
                foreach (var rec in categoryDistribution.Values)
                {
                    rec.TotalSharePercentage = (rec.USDValue / totalUsdValue) * 100;
                }
            }

            return categoryDistribution;
        }, cancellationToken);
    }

    /// <summary>
    /// Create daily NAV's for the whole period.
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="bookingPeriod"></param>
    /// <param name="forceRecalculation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> CalcDailyNavsForPeriod(Guid fundId, string bookingPeriod, bool forceRecalculation = false, CancellationToken cancellationToken = default)
    {
        var fund = await _db.Funds.SingleOrDefaultAsync(f => f.Id == fundId, cancellationToken);

        if (fund == null)
            return false;

        var startDate = _bookingPeriodHelper.GetPeriodStartDateTime(bookingPeriod).AddDays(1);
        var endDate = _bookingPeriodHelper.GetPeriodEndDateTime(bookingPeriod);
        if (startDate.ToUniversalTime() < fund.DateStart.ToUniversalTime())
        {
            startDate = new DateTimeOffset(fund.DateStart.ToUniversalTime());
        }
        if (fund.DateEnd is DateTime dtEnd && endDate.ToUniversalTime() > dtEnd.ToUniversalTime())
        {
            endDate = new DateTimeOffset(dtEnd.ToUniversalTime());
        }
        if (endDate > DateTimeOffset.UtcNow)
        {
            endDate = DateTimeOffset.UtcNow;
        }

        var oldNavs = await _db.Navs
            .Where(nav =>
                nav.FundId == fundId &&
                nav.Type == NavType.Daily &&
                nav.Date >= startDate.UtcDateTime.Date &&
                nav.Date <= endDate.UtcDateTime.Date)
            .Include(nav => nav.CurrencyRate)
            .ToListAsync(cancellationToken);

        // Create loop for all days in period
        while (startDate <= endDate && !cancellationToken.IsCancellationRequested)
        {
            var navDate = new DateTime(_bookingPeriodHelper.NavDate(startDate).Ticks, DateTimeKind.Utc);

            if (forceRecalculation || !oldNavs.Any(nav => nav.Date == navDate))
            {
                await CreateDailyNAV(fundId, navDate, cancellationToken);
            }
            startDate = startDate.AddDays(1);
        }

        if (forceRecalculation)
        {
            _db.Navs.RemoveRange(oldNavs);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    private async Task<Transfer[]> GetAllHoldingTransfers(IList<Holding> holdings, CancellationToken cancellationToken)
    {
        var holdingIds = holdings.Select(h => h.Id).ToArray();

        return await _db.Transfers
            .Where(t => holdingIds.Contains(t.HoldingId))
            .OrderBy(t => t.DateTime)
            .ToArrayAsync(cancellationToken);
    }

    /// <summary>
    /// Get al the trades in the period.
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="closeBookingPeriod"></param>
    /// <param name="periodEnd"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Trade[]> GetAllPeriodTrades(Guid fundId, string closeBookingPeriod, DateTimeOffset periodEnd, CancellationToken cancellationToken)
    {
        return await _db.Trades
            .Where(trade => trade.Order.OrderFundings.Any(f => f.FundId == fundId && f.OrderAmount != 0) &&
                (trade.BookingPeriod == closeBookingPeriod ||
                    (
                        string.IsNullOrWhiteSpace(trade.BookingPeriod) &&
                        //trade.DateTime >= periodStart.UtcDateTime && <- Also take older trades that have not been booked before
                        trade.DateTime < periodEnd.UtcDateTime
                    )
                )
            )
            .Include(t => t.Order)
            .ThenInclude(o => o.OrderFundings.Where(f => f.FundId == fundId))
            .OrderBy(t => t.DateTime)
            .ThenBy(t => t.Order.OrderNumber)
            .ToArrayAsync(cancellationToken);
    }

    private async Task<IList<Holding>> CalcPeriodEndBalances(Guid fundId, string closeBookingPeriod, DateTimeOffset periodEnd, CancellationToken cancellationToken)
    {
        // Get al the trades in the period and make sure for all currencies
        // holdings are created.
        Trade[] trades = await GetAllPeriodTrades(fundId, closeBookingPeriod, periodEnd, cancellationToken);
        IList<Holding> holdings = await GetAllFundHoldings(fundId, closeBookingPeriod, trades, cancellationToken);

        // Also get all transfers for the holdings in the period
        Transfer[] transfers = await GetAllHoldingTransfers(holdings, cancellationToken);

        // Calculate the end balances for the holdings
        CalcEndBalances(holdings, transfers, trades, closeBookingPeriod);

        return holdings;
    }

    private async Task<Holding> CreateHolding(Guid fundId, Guid? cryptoCurrencyId, string currencyISOCode, string bookingPeriod, CancellationToken cancellationToken = default)
    {
        Holding holding = new()
        {
            FundId = fundId,
            CryptoId = cryptoCurrencyId,
            CurrencyISOCode = currencyISOCode,
            BookingPeriod = bookingPeriod,
        };
        var startDate = _bookingPeriodHelper.GetPeriodStartDateTime(bookingPeriod);

        if (cryptoCurrencyId != null)
        {
            // Find latest price
            var listing = await _cryptoService.GetListingByDate((Guid)cryptoCurrencyId, startDate, _cryptoService.PreferedListingSource, true, cancellationToken);
            if (listing != null)
            {
                holding.ListingId = listing.Id;
                holding.StartUSDPrice = listing.USDPrice;
                holding.StartBTCPrice = listing.BTCPrice;
            }
        }
        else if (!string.IsNullOrEmpty(currencyISOCode))
        {
            // Find latest rate
            var rate = await _currencyService.GetCurrencyRatingByDate(currencyISOCode, startDate, cancellationToken);
            var btcListing = await _cryptoService.GetMostRecentListing(_cryptoService.BtcGuid, _cryptoService.PreferedListingSource, cancellationToken);
            if (rate != null)
            {
                holding.CurrencyRateId = rate.Id;
                holding.StartUSDPrice = rate.USDRate;
                holding.StartBTCPrice = 1 / (btcListing.USDPrice / rate.USDRate);
            }
        }

        await _db.Holdings.AddAsync(holding, cancellationToken);

        return holding;
    }

    /// <summary>
    /// Gets all the holdings for the bookingperiod, and creates holdings for
    /// currencies where trades have transfered to, when no holding yet exists.
    ///
    /// Make sure that the trades include information about the order and order
    /// funding. This is used to filter the orders when no funding from the
    /// selected fund is used.
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="bookingPeriod"></param>
    /// <param name="trades"></param>
    /// <returns></returns>
    private async Task<IList<Holding>> GetAllFundHoldings(Guid fundId, string bookingPeriod, Trade[] trades, CancellationToken cancellationToken = default)
    {
        List<Guid> cryptos = new();
        cryptos.AddRange(trades
            .Where(t => t.Order.OrderFundings.Any(f => f.FundId == fundId && f.OrderAmount != 0))
            .Select(t => t.Order.QuoteAssetId)
            .Distinct()
            .Where(c => !cryptos.Contains(c)));
        cryptos.AddRange(trades
            .Where(t => t.Order.OrderFundings.Any(f => f.FundId == fundId && f.OrderAmount != 0))
            .Select(t => t.Order.BaseAssetId)
            .Distinct()
            .Where(c => !cryptos.Contains(c)));

        // Get or create holdings for all the currencies used in trades
        List<Holding> holdings = await _db.Holdings
            .Where(h => h.FundId == fundId && h.BookingPeriod == bookingPeriod)
            .ToListAsync(cancellationToken);

        foreach (var cryptoId in cryptos)
        {
            if (!holdings.Any(h => h.CryptoId == cryptoId))
            {
                var holding = await CreateHolding(fundId, cryptoId, null, bookingPeriod, cancellationToken);
                holdings.Add(holding);
            }
        }

        return holdings;
    }

    /// <summary>
    /// Calculates the end balances of the holdings. Here all trades and
    /// transfers are added and subtracted from the holding balance, without
    /// adding or subtracting the in- out-flow transfers.
    /// </summary>
    /// <param name="holdings"></param>
    /// <param name="transfers"></param>
    /// <param name="trades"></param>
    /// <param name="closeBookingPeriod"></param>
    private static void CalcEndBalances(IEnumerable<Holding> holdings, IEnumerable<Transfer> transfers, IEnumerable<Trade> trades, string closeBookingPeriod)
    {
        // Then Calculate all the balances from start to end
        foreach (var holding in holdings)
        {
            // Reset the end-balance
            holding.EndBalance = holding.StartBalance;

            // Add or subtract the transfers
            foreach (var transfer in transfers.Where(t => t.HoldingId == holding.Id))
            {
                // The in- and out-flow is only calculated after the NAV is calculated
                if (transfer.TransactionType == TransactionType.Outflow ||
                    transfer.TransactionType == TransactionType.Inflow)
                {
                    continue;
                }

                transfer.BookingPeriod = closeBookingPeriod;
                switch (transfer.Direction)
                {
                    case TransferDirection.In:
                        holding.EndBalance += transfer.TransferAmount;
                        break;
                    case TransferDirection.Out:
                        holding.EndBalance -= transfer.TransferAmount;
                        break;
                }
            }
            // Subtract all the fees
            holding.EndBalance -= transfers.Where(t => t.FeeHoldingId == holding.Id).Sum(t => t.TransferFee);
        }

        // Then walk through the trades and transfer the holding balances
        foreach (var trade in trades)
        {
            var fromHolding = holdings.Single(h => h.CryptoId != null && h.CryptoId == trade.Order.QuoteAssetId);
            var toHolding = holdings.Single(h => h.CryptoId != null && h.CryptoId == trade.Order.BaseAssetId);
            var feeHolding = holdings.Single(h => h.CryptoId != null && h.CryptoId == trade.FeeCurrencyId);

            var fundingPercentage = trade.Order.OrderFundings.SingleOrDefault(of => of.FundId == fromHolding.FundId)?.OrderPercentage ?? 0;

            if (fundingPercentage > 0)
            {
                switch (trade.Order.Direction)
                {
                    case OrderDirection.Sell:
                        fromHolding.EndBalance += (trade.Total * fundingPercentage / 100);
                        toHolding.EndBalance -= (trade.Executed * fundingPercentage / 100);
                        break;
                    default:
                        fromHolding.EndBalance -= (trade.Total * fundingPercentage / 100);
                        toHolding.EndBalance += (trade.Executed * fundingPercentage / 100);
                        break;
                }

                // Only apply the bookingperiod when the funding percentage != 0 and the bookingperiod is not empty
                if (!string.IsNullOrEmpty(closeBookingPeriod))
                {
                    trade.BookingPeriod = closeBookingPeriod;
                }
                feeHolding.EndBalance -= (trade.Fee * fundingPercentage / 100);
            }
        }
    }

    /// <summary>
    /// Calculates the BTC and USD value for all the holdingsusing the accurate
    /// currency rating or crypto listing. If one of the holdings has no
    /// listing, an error is thrown.
    /// </summary>
    /// <param name="holdings"></param>
    /// <param name="timestamp"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<HoldingSum> CalcEndValues(IEnumerable<Holding> holdings, DateTimeOffset timestamp, CancellationToken cancellationToken)
    {
        HoldingSum result = new();

        var btc = await _cryptoService.GetBtcCryptoCurrencyAsync(cancellationToken);
        var btcListing = await _cryptoService.GetListingByDate(btc.Id, timestamp, _cryptoService.PreferedListingSource, false, cancellationToken);

        foreach (var holding in holdings)
        {
            // Calc the USD and BTC values and close the period
            if (!string.IsNullOrEmpty(holding.CurrencyISOCode))
            {
                var rate = await _currencyService.GetCurrencyRatingByDate(holding.CurrencyISOCode, timestamp, cancellationToken);
                if (rate == null)
                {
                    _errorInformationManager.AddValidationError(HttpStatusCode.Conflict, $"Can not close booking period because there is no rating for the currency {holding.CurrencyISOCode}.");
                    continue;
                }
                holding.CurrencyRateId = rate.Id;
                holding.EndUSDPrice = rate.USDRate;
                holding.EndBTCPrice = holding.EndUSDPrice / btcListing.USDPrice;
            }

            if (holding.CryptoId != null)
            {
                var listing = await _cryptoService.GetListingByDate((Guid)holding.CryptoId, timestamp, _cryptoService.PreferedListingSource, false, cancellationToken);
                if (listing == null)
                {
                    var crypto = await _cryptoService.GetCryptoCurrencyAsync((Guid)holding.CryptoId, cancellationToken);
                    _errorInformationManager.AddValidationError(HttpStatusCode.Conflict, $"Can not close booking period because there is no listing for the cryptocurrency {crypto.Symbol}.");
                    continue;
                }
                holding.ListingId = listing.Id;
                holding.EndUSDPrice = listing.USDPrice;
                holding.EndBTCPrice = listing.BTCPrice;
            }

            if (holding.SharesFundId != null)
            {
                // Calculate the BTC price from the usd rate
                var nav = await GetNavByDate((Guid)holding.SharesFundId, timestamp, cancellationToken);
                if (nav == null)
                {
                    var fund = await _db.Funds.SingleAsync(f => f.Id == holding.SharesFundId, cancellationToken);
                    _errorInformationManager.AddValidationError(HttpStatusCode.Conflict, $"Can not close booking period because there is no NAV available for the shares in the fund {fund.FundName}.");
                    continue;
                }
                holding.EndUSDPrice = nav.ShareNAV / nav.CurrencyRate.USDRate;
                holding.EndBTCPrice = holding.EndUSDPrice / btcListing.USDPrice;
            }

            result.BTCValue += (holding.EndBTCPrice * holding.EndBalance);
            result.USDValue += (holding.EndUSDPrice * holding.EndBalance);
        }

        _errorInformationManager.ThrowOnErrors();

        return result;
    }

    /// <summary>
    /// Calculates the Gross Asset Value and Netto Asset Value for the fund and
    /// creates a new NAV record.
    /// </summary>
    /// <param name="fund"></param>
    /// <param name="totalUSDValue"></param>
    /// <param name="navType"></param>
    /// <param name="date"></param>
    /// <param name="bookingPeriod"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Nav> CreateNav(Fund fund, decimal totalUSDValue, NavType navType, DateTime date, string bookingPeriod, CancellationToken cancellationToken)
    {
        var dtOffset = _bookingPeriodHelper.DailyNavEndDateTime(date);
        var reportinCurrencyRate = await _currencyService.GetCurrencyRatingByDate(fund.ReportingCurrencyCode, dtOffset, cancellationToken);

        // When there is already a calculation done, then the HWM values are
        // overwritten. We have to get the original one from that calculation
        // then.
        var originalNav = await _db.Navs
                .Where(nav => nav.FundId == fund.Id && nav.Type == NavType.Period && nav.BookingPeriod == bookingPeriod)
                .Include(nav => nav.CurrencyRate)
                .OrderByDescending(nav => nav.DateTime)
                .FirstOrDefaultAsync(cancellationToken);
        var oldShareHwm = originalNav?.ShareHWM ?? fund.ShareValueHWM;
        if (oldShareHwm < 1)
            oldShareHwm = 1; // High water mark can never be below 1, because that's the starting point.

        // The total shares will be calculated from the previous booking period.
        // The reported shares and all the in- and outflow from that period are
        // summed.
        var previousBookingPeriod = _bookingPeriodHelper.GetPreviousBookingPeriod(bookingPeriod);
        var previousNav = await _db.Navs
                .Where(nav => nav.FundId == fund.Id && nav.Type == NavType.Period && nav.BookingPeriod == previousBookingPeriod)
                .Include(nav => nav.CurrencyRate)
                .OrderByDescending(nav => nav.DateTime)
                .FirstOrDefaultAsync(cancellationToken);

        int totalShares = 0;
        decimal totalValue = totalUSDValue / reportinCurrencyRate.USDRate;
        decimal shareGross = 1;
        if (previousNav == null)
        {
            // This is the first NAV for the fund. When this is the first time
            // a NAV calculation is done, take the TotalShares from the fund as
            // it is filled in manually. If there already was a NAV before,
            // take the number of shares from there.
            totalShares = originalNav?.TotalShares ?? fund.TotalShares;
        }
        else
        {
            totalShares = previousNav.TotalShares + previousNav.InOutShares;
            oldShareHwm = Math.Max(previousNav.ShareHWM, previousNav.ShareNAV);
        }

        if (totalShares == 0)
        {
            // Calculate the totalshares from teh value and gross value of 1
            totalShares = (int)Math.Floor(totalValue / shareGross);
        }
        else
        {
            // Otherwise calculate the gross share value from the totalvalue
            // and the number of shares.
            shareGross = totalValue / totalShares; // This is the gross value
        }

        decimal adminstrationFee = 0;
        if (_bookingPeriodHelper.BookAdministrationFee(fund.AdministrationFeeFrequency, bookingPeriod))
        {
            decimal feePercentage = (decimal)fund.AdministrationFee / fund.AdministrationFeeFrequency;
            adminstrationFee = totalValue / 100 * feePercentage;
        }
        var sharePerformanceFee = shareGross > oldShareHwm // When a new HWM is reached, the profit percentage is taken off the GrossNav
                ? ((shareGross - oldShareHwm) * fund.PerformanceFee / 100)
                : 0;
        var shareNav = totalShares > 0
            ? ((totalValue - adminstrationFee) / totalShares) - sharePerformanceFee
            : 1;

        return new()
        {
            FundId = fund.Id,
            DateTime = DateTime.UtcNow,
            Type = navType,
            Date = new DateTime(date.Ticks, DateTimeKind.Utc),
            BookingPeriod = bookingPeriod,
            TotalShares = totalShares,
            TotalValue = totalValue,
            ShareHWM = oldShareHwm,
            ShareGross = shareGross,
            ShareNAV = shareNav,
            AdministrationFee = adminstrationFee,
            PerformanceFee = sharePerformanceFee * totalShares,
            CurrencyRateId = reportinCurrencyRate.Id,
            CurrencyRate = reportinCurrencyRate
        };
    }

    private async Task<int> CalcShares(Holding holding, Transfer transfer, Nav nav, CancellationToken cancellationToken = default)
    {
        // The standard way of working is that the inflow/outflow is done right
        // after the NAV's are calculated. The money should be transfered at
        // least 4 days before the NAV calculation, but the booking of the
        // in-/out-flow is done right after, or during the month. The exchange
        // rates that are used are for this reason always on the day of the NAV.

        // Make sure there is a price on the holding to calculate the shares.
        await EnsureHoldingStartPrice(holding, cancellationToken);

        // Now calculate the current holding value back to the reporting
        // currency, and calaculate the shares for the amount.
        var transferUsdValue = transfer.TransferAmount * holding.EndUSDPrice;
        var transferReportingValue = transferUsdValue / nav.CurrencyRate.USDRate;

        // And calculate the shares
        var shares = (int)Math.Round(transferReportingValue / nav.ShareNAV);

        return shares;
    }

    private async Task<decimal> CalcOutflowAmount(Holding holding, Transfer transfer, Nav nav, CancellationToken cancellationToken = default)
    {
        // The standard way of working is that the inflow/outflow is done right
        // after the NAV's are calculated. In case of outflow, the transfer can
        // state the number of shares, and the value must be calculated.

        // Make sure there is a price on the holding to calculate the shares.
        await EnsureHoldingStartPrice(holding, cancellationToken);

        // And calculate the value of the shares
        return (transfer.Shares * nav.ShareNAV * nav.CurrencyRate.USDRate) / holding.EndUSDPrice;
    }

    /// <summary>
    /// Ensures a price for the holding currency
    /// </summary>
    /// <param name="holding"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    private async Task EnsureHoldingStartPrice(Holding holding, CancellationToken cancellationToken = default)
    {
        if (holding.StartUSDPrice > 0 && holding.StartBTCPrice > 0) return;

        var startDateOffset = new DateTimeOffset(holding.StartDateTime);

        if (holding.CryptoId != null)
        {
            var listing = await _cryptoService.GetListingByDate((Guid)holding.CryptoId, startDateOffset, _cryptoService.PreferedListingSource, true, cancellationToken) ??
                throw new RestException(HttpStatusCode.Conflict, new ErrorInformationItem
                {
                    Code = ErrorCodesStore.ReferenceCheckFailed,
                    Description = $"Could not find a price for the holding with crypto currency {holding.CryptoCurrency.Symbol}"
                });
            holding.StartUSDPrice = listing.USDPrice;
            holding.StartBTCPrice = listing.BTCPrice;
        }

        if (holding.CurrencyISOCode != null)
        {
            var rate = await _currencyService.GetCurrencyRatingByDate(holding.CurrencyISOCode, startDateOffset, cancellationToken);
            var btcListing = await _cryptoService.GetListingByDate(_cryptoService.BtcGuid, startDateOffset, _cryptoService.PreferedListingSource, true, cancellationToken);
            if (rate == null)
                throw new RestException(HttpStatusCode.Conflict, new ErrorInformationItem
                {
                    Code = ErrorCodesStore.ReferenceCheckFailed,
                    Description = $"Could not find a price for the holding with currency {holding.CurrencyISOCode}"
                });

            holding.StartUSDPrice = rate.USDRate;
            holding.StartBTCPrice = 1 / (btcListing.USDPrice / rate.USDRate);
        }
    }

    #region Fund Categories

    /// <summary>
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IList<FundCategory>> GetFundCategories(Guid fundId, CancellationToken cancellationToken)
    {
        var fundCategories = await _db.FundCategories
            .AsNoTracking()
            .Where(fc => fc.FundId == fundId)
            .Include(fc => fc.Category)
            .OrderBy(fc => fc.Category.Group)
            .ThenBy(fc => fc.Category.Name)
            .AsSingleQuery()
            .ToListAsync(cancellationToken);

        return fundCategories;
    }


    public async Task<FundCategory> GetFundCategory(Guid fundId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var fundCategory = await _db.FundCategories
            .AsNoTracking()
            .Where(fc => fc.FundId == fundId && fc.CategoryId == categoryId)
            .Include(fc => fc.Category)
            .AsSingleQuery()
            .SingleOrDefaultAsync(cancellationToken);

        return fundCategory;
    }

    #endregion

}

