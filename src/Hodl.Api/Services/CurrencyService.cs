namespace Hodl.Api.Services;

public class CurrencyService : ICurrencyService
{
    private readonly HodlDbContext _db;
    private readonly ICryptoCurrencyService _cryptoService;

    public CurrencyService(
        HodlDbContext dbContext,
        ICryptoCurrencyService cryptoService)
    {
        _db = dbContext;
        _cryptoService = cryptoService;
    }

    /// <summary>
    /// Selects all real world currencies are registered in the system.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IList<Currency>> GetUsedCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Currencies
            .Where(c => c.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> AppendRates(IEnumerable<CurrencyRate> rates, CancellationToken cancellationToken = default)
    {
        // Append rates only when the timestamp (when date) differs from saved rates.
        List<CurrencyRate> newRates = new();
        foreach (CurrencyRate rate in rates)
        {
            if (!await _db.CurrencyRates
                .AnyAsync(cr => cr.CurrencyISOCode == rate.CurrencyISOCode && cr.TimeStamp.Equals(rate.TimeStamp),
                cancellationToken))
            {
                newRates.Add(rate);
            }
        }

        // Get the last date to check if we have to update the holdings rates
        var lastRating = await _db.CurrencyRates
            .OrderByDescending(r => r.TimeStamp)
            .FirstOrDefaultAsync(cancellationToken);

        await _db.CurrencyRates.AddRangeAsync(newRates, cancellationToken);
        var added = await _db.SaveChangesAsync(cancellationToken);

        // Add the latest rates to the active holdings.
        if (newRates.Any(r => r.TimeStamp >= lastRating.TimeStamp))
        {
            var btcListing = await _cryptoService.GetMostRecentListing(_cryptoService.BtcGuid, _cryptoService.PreferedListingSource, cancellationToken);

            var latestRates = newRates
                .OrderBy(r => r.TimeStamp)
                .DistinctBy(r => r.CurrencyISOCode)
                .ToArray();

            var ratesCurrencyISOCodes = latestRates.Select(l => l.CurrencyISOCode).Distinct().ToList();

            // Add the listings to the active holdings
            var holdings = await _db.Holdings
                .Where(h =>
                    !string.IsNullOrEmpty(h.CurrencyISOCode) &&
                    h.PeriodClosedDateTime == null &&
                    ratesCurrencyISOCodes.Contains(h.CurrencyISOCode))
                .ToListAsync(cancellationToken);

            foreach (var holding in holdings)
            {
                var rate = latestRates.FirstOrDefault(r => r.CurrencyISOCode.Equals(holding.CurrencyISOCode));

                holding.CurrencyRateId = rate.Id;
                holding.EndUSDPrice = rate.USDRate;
                holding.EndBTCPrice = 1 / (btcListing.USDPrice / rate.USDRate);
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        return added;
    }

    public async Task<CurrencyRate> GetMostRecentRating(string isoCode, CancellationToken cancellationToken = default)
    {
        return await _db.CurrencyRates
            .Where(r => r.CurrencyISOCode == isoCode)
            .OrderByDescending(r => r.TimeStamp)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CurrencyRate> GetFirstRating(string isoCode, CancellationToken cancellationToken = default)
    {
        return await _db.CurrencyRates
            .Where(l => l.CurrencyISOCode == isoCode)
            .OrderBy(l => l.TimeStamp)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CurrencyRate> GetCurrencyRatingByDate(string isoCode, DateTimeOffset datetime, CancellationToken cancellationToken = default)
    {
        return await _db.CurrencyRates
            .Where(r => r.CurrencyISOCode == isoCode && r.TimeStamp < datetime.UtcDateTime)
            .OrderByDescending(r => r.TimeStamp)
            .FirstOrDefaultAsync(cancellationToken) ??
            await GetFirstRating(isoCode, cancellationToken);
    }
}
