namespace Hodl.Api.Interfaces;

public interface ICurrencyService
{
    Task<IList<Currency>> GetUsedCurrenciesAsync(CancellationToken cancellationToken);

    Task<CurrencyRate> GetMostRecentRating(string isoCode, CancellationToken cancellationToken = default);

    Task<CurrencyRate> GetFirstRating(string isoCode, CancellationToken cancellationToken = default);

    Task<CurrencyRate> GetCurrencyRatingByDate(string isoCode, DateTimeOffset datetime, CancellationToken cancellationToken = default);

    Task<int> AppendRates(IEnumerable<CurrencyRate> rates, CancellationToken cancellationToken);
}
