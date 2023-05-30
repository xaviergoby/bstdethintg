namespace Hodl.Api.Interfaces;

public interface ICryptoCurrencyService
{

    Guid BtcGuid { get; }

    string PreferedListingSource { get; }

    #region CrytpoCurrency

    /// <summary>
    /// Gets the Bitcoin CryptoCurrency from the database.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CryptoCurrency> GetBtcCryptoCurrencyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Selects all Crypto currencies that are actively used in funds. This requires
    /// a Holding record that does not have a closed bookingperiod.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IList<CryptoCurrency>> GetUsedCurrenciesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Select paged set of used currencies.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PagingModel<CryptoCurrency>> GetUsedCurrenciesAsync(int page, int? itemsPerPage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a crypto currency by Symbol/Ticker.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CryptoCurrency> GetCryptoCurrencyBySymbol(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the cryptocurrency by ID
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CryptoCurrency> GetCryptoCurrencyAsync(Guid cryptoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search crypto currencies. Returns a paged resultlist.
    /// </summary>
    /// <param name="search"></param>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <returns></returns>
    Task<PagingModel<CryptoCurrency>> Search(string search, int page, int? itemsPerPage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new crypto currency.
    /// </summary>
    /// <param name="newCrypto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AddCryptoCurrency(CryptoCurrency crypto, CancellationToken cancellationToken = default);

    Task UpdateCryptoCurrency(CryptoCurrency crypto, CancellationToken cancellationToken = default);

    Task DeleteCurrency(Guid cryptoId, CancellationToken cancellationToken = default);

    Task<int> ImportCoins(int startIndex, int endIndex, CancellationToken cancellationToken = default);
    #endregion

    #region Listings
    /// <summary>
    /// Gets the first available listing for a crypto currency.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Listing> GetFirstListing(Guid cryptoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest listing for a crypto currency.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Listing> GetMostRecentListing(Guid cryptoId, string preferedSource, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all latest listings for the given crypto currency id's
    /// </summary>
    /// <param name="cryptos"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<Listing>> GetMostRecentListings(Guid cryptoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest listing before the given datetime. If no listing before 
    /// the datetime can be found, the first available listing is returned or 
    /// the default value (null).
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="datetime"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Listing> GetListingByDate(Guid cryptoId, DateTimeOffset datetime, string preferedSource, bool importHistory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Return the pages listings by crypto currency.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="dateFrom"></param>
    /// <param name="dateTo"></param>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PagingModel<Listing>> GetListings(Guid cryptoId, DateTime dateFrom, DateTime dateTo, int page, int? itemsPerPage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single listing by cryptoId and Listing Id.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Listing> GetListingById(Guid cryptoId, long id, CancellationToken cancellationToken = default);

    Task UpdateListing(Listing listing, CancellationToken cancellationToken = default);

    Task AddListing(Listing listing, CancellationToken cancellationToken = default);

    Task DeleteListing(Guid cryptoId, long ListingId, CancellationToken cancellationToken = default);

    Task<int> AppendListings(IEnumerable<Listing> listings, CancellationToken cancellationToken = default);

    Task<bool> AddLatestListings(CancellationToken cancellationToken = default);

    Task<bool> AddListingsHistory(CryptoCurrency crypto, DateTime startDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Routine that removes listings older than cleanupDelay days. Only hourly 
    /// listings will be saved.
    /// </summary>
    /// <param name="cleanupDelay"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CleanupListings(int cleanupDelay, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the appsettings for the currency to get historical quotes.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> TriggerListingsHistory(string symbol, CancellationToken cancellationToken = default);

    Task<IEnumerable<Listing>> SelectListingsBySource(string source, CancellationToken cancellationToken = default);
    #endregion

    #region TokenContracts
    /// <summary>
    /// Imports Token Contract Addresses for the given currency and returns the 
    /// list of contract addresses on the stored blockchain networks.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<TokenContract>> ImportTokenContracts(CryptoCurrency crypto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports Token Contract addresses for multiple given currencies and 
    /// returns the list of contract addresses on the stored blockchain 
    /// networks.
    /// </summary>
    /// <param name="cryptoIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<TokenContract>> ImportTokenContracts(IEnumerable<CryptoCurrency> cryptos, CancellationToken cancellationToken = default);

    #endregion
}
