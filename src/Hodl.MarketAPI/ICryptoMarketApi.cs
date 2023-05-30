using Hodl.MarketAPI.Models;

namespace Hodl.MarketAPI;

public interface ICryptoMarketApi
{
    string Source { get; }

    Task<IEnumerable<MarketCryptoCurrency>> GetCryptoCurrencies(int startIndex, int endIndex, CancellationToken cancellationToken);

    Task<byte[]> GetIcon(string symbol, CancellationToken cancellationToken);

    Task<IEnumerable<MarketListing>> GetHistoricalListings(MarketCryptoCurrency crypto, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);

    Task<IEnumerable<MarketListing>> GetLatestListings(IEnumerable<MarketCryptoCurrency> cryptos, CancellationToken cancellationToken);

    Task<int> GetCurrencyRank(string symbol, CancellationToken cancellationToken);

    Task<IEnumerable<MarketTokenContact>> GetTokenContracts(IEnumerable<string> symbols, CancellationToken cancellationToken);
}
