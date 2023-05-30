using Hodl.Api.ViewModels.CurrencyModels;
using Hodl.MarketAPI;

namespace Hodl.Api.Controllers.Currencies;

[ApiController]
[Route("cryptos")]
public class CryptoCurrencyController : BaseController
{
    private readonly ICryptoCurrencyService _cryptoCurrencyService;
    private readonly IBookingPeriodHelper _bookingPeriodHelper;

    public CryptoCurrencyController(
        ICryptoCurrencyService cryptoCurrencyService,
        IBookingPeriodHelper bookingPeriodHelper,
        IMapper mapper,
        ILogger<CryptoCurrencyController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _cryptoCurrencyService = cryptoCurrencyService;
        _bookingPeriodHelper = bookingPeriodHelper;
    }

    #region CryptoCurrencies
    /// <summary>
    /// Private function to add latest listing to the cryptocurrency.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="preferedSource"></param>
    /// <returns></returns>
    private async Task<ListingViewModel> GetCurrencyListing(
        Guid cryptoId, string preferedSource, CancellationToken ct)
    {
        return _mapper.Map<ListingViewModel>(await _cryptoCurrencyService.GetMostRecentListing(cryptoId, preferedSource, ct));
    }

    /// <summary>
    /// Gets the 100 latest listings for the given currency
    /// </summary>
    /// <param name="crypto"></param>
    /// <returns></returns>
    private async Task<ICollection<ListingViewModel>> GetCurrencyListings(
        CryptoCurrency crypto, CancellationToken ct)
    {
        var listings = await _cryptoCurrencyService
            .GetMostRecentListings(crypto.ListingCryptoId ?? crypto.Id, ct);

        return listings
            .Select(l => _mapper.Map<ListingViewModel>(l))
            .ToArray();
    }

    /// <summary>
    /// Gets list of all crypto currencies.
    /// </summary>
    /// <param name="search">Search string to look for in symbol or name</param>
    /// <param name="page">Selected page (optional)</param>
    /// <param name="itemsPerPage">Number of items per page (optional, default 20)</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<PagingViewModel<CryptoCurrencyListViewModel>>> Get(
        string search, int page, int? itemsPerPage, CancellationToken ct)
    {
        // Gotta first declare this var (with the correct type) due to scoping matteres
        var pageResult = await _cryptoCurrencyService.Search(search, page, itemsPerPage, ct);
        var pageResultView = _mapper.Map<PagingViewModel<CryptoCurrencyListViewModel>>(pageResult);

        // And add current/latest listings on the cryptocurrency
        foreach (var crypto in pageResultView.Items)
        {
            crypto.Listing = await GetCurrencyListing(crypto.ListingCryptoId ?? crypto.Id, _cryptoCurrencyService.PreferedListingSource, ct);
        }

        return Ok(pageResultView);
    }

    /// <summary>
    /// Gets list of used crypto currencies. These are crypto's that are linked to holdings.
    /// </summary>
    /// <param name="page">Selected page (optional)</param>
    /// <param name="itemsPerPage">Number of items per page (optional, default 20)</param>
    /// <returns></returns>
    [HttpGet]
    [Route("used")]
    public async Task<ActionResult<PagingViewModel<CryptoCurrencyListViewModel>>> GetUsedCurrencies(
        int page, int? itemsPerPage, CancellationToken ct)
    {
        // Gotta first declare this var (with the correct type) due to scoping matteres
        var pageResult = await _cryptoCurrencyService.GetUsedCurrenciesAsync(page, itemsPerPage, ct);
        var pageResultView = _mapper.Map<PagingViewModel<CryptoCurrencyListViewModel>>(pageResult);

        // And add current/latest listings on the cryptocurrency
        foreach (var crypto in pageResultView.Items)
        {
            crypto.Listing = await GetCurrencyListing(crypto.ListingCryptoId ?? crypto.Id, _cryptoCurrencyService.PreferedListingSource, ct: ct);
        }

        return Ok(pageResultView);
    }

    /// <summary>
    /// Add a new crypto currency.
    /// </summary>
    /// <param name="crypto">The crypto currency to be added</param>
    /// <returns>The stored cryptocurrency including the new identifier</returns>
    [HttpPost]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<CryptoCurrencyListViewModel>> PostCurrency([FromBody] CryptoCurrencyEditViewModel crypto)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        // No exceptions, so proceed with the actual handling of the request
        var newCrypto = _mapper.Map<CryptoCurrency>(crypto);

        await _cryptoCurrencyService.AddCryptoCurrency(newCrypto);

        return Ok(_mapper.Map<CryptoCurrencyListViewModel>(newCrypto));
    }

    /// <summary>
    /// Gets the crypto currency using the identifier including with latest listings.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <returns>The selected crypto currency</returns>
    [HttpGet]
    [Route("{cryptoId}")]
    public async Task<ActionResult<CryptoCurrencyDetailViewModel>> GetCurrency(Guid cryptoId, CancellationToken ct)
    {
        var crypto = await _cryptoCurrencyService.GetCryptoCurrencyAsync(cryptoId, ct);
        if (crypto == null) return NotFound();

        var mappedCrypto = _mapper.Map<CryptoCurrencyDetailViewModel>(crypto);

        if (crypto.ListingCryptoId != null)
        {
            var listingCrypto = await _cryptoCurrencyService.GetCryptoCurrencyAsync((Guid)crypto.ListingCryptoId, ct);
            mappedCrypto.ListingReference = _mapper.Map<CryptoCurrencyListViewModel>(listingCrypto);
        }

        mappedCrypto.Listing = await GetCurrencyListing(crypto.Id, _cryptoCurrencyService.PreferedListingSource, ct);
        mappedCrypto.Listings = await GetCurrencyListings(crypto, ct);

        return Ok(mappedCrypto);
    }

    /// <summary>
    /// Gets the crypto currency icon as png image.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <returns>The selected crypto currency</returns>
    [HttpGet]
    [Route("{cryptoId}/icon"), ResponseCache(Duration = 3600)]
    [AllowAnonymous]
    public async Task<IActionResult> GetCurrencyIcon(Guid cryptoId, CancellationToken ct)
    {
        // Get the bytes from the DB record
        var crypto = await _cryptoCurrencyService.GetCryptoCurrencyAsync(cryptoId, ct);

        if (crypto == null) return NotFound();

        if (crypto.Icon == null || crypto.Icon.Length == 0) return NotFound();

        return File(crypto.Icon, "image/png");
    }

    /// <summary>
    /// Modify an existing crypto currency.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="crypto"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{cryptoId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutCurrency(Guid cryptoId, [FromBody] CryptoCurrencyEditViewModel crypto)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedCrypto = await _cryptoCurrencyService.GetCryptoCurrencyAsync(cryptoId);

        if (storedCrypto == null)
            return NotFound();

        // Apply and save the changes
        _mapper.Map(crypto, storedCrypto);

        await _cryptoCurrencyService.UpdateCryptoCurrency(storedCrypto);

        return Ok();
    }

    /// <summary>
    /// Remove the crypto currency.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("{cryptoId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteCurrency(Guid cryptoId)
    {
        await _cryptoCurrencyService.DeleteCurrency(cryptoId);

        return Ok();
    }

    #endregion

    #region Listings

    /// <summary>
    /// Gets list of all currencies listings within a period of time. 
    /// </summary>
    /// <param name="from">From date given as YYYY-MM-DD or YYYY-MM-DDTHH:mm:ss</param>
    /// <param name="to">From date given as YYYY-MM-DD or YYYY-MM-DDTHH:mm:ss</param>
    /// <returns></returns>
    [HttpGet]
    [Route("{cryptoId}/listings")]
    public async Task<ActionResult<PagingViewModel<ListingViewModel>>> GetListings(
        Guid cryptoId, string from, string to, int page, int? itemsPerPage, CancellationToken ct)
    {
        var dateFrom = from.ParamToUtcDate(DateTime.Today.AddMonths(-1));
        var dateTo = to.ParamToUtcDate(DateTime.MaxValue);

        // Create a paged resultset
        var pageResult = await _cryptoCurrencyService.GetListings(cryptoId, dateFrom, dateTo, page, itemsPerPage, ct);

        return Ok(_mapper.Map<PagingViewModel<ListingViewModel>>(pageResult));
    }

    [HttpGet]
    [Route("{cryptoId}/listings/{id}")]
    public async Task<ActionResult<ListingViewModel>> GetListings(Guid cryptoId, long id, CancellationToken ct)
    {
        var listing = await _cryptoCurrencyService.GetListingById(cryptoId, id, ct);

        if (listing == null)
            return NotFound();

        return Ok(_mapper.Map<ListingViewModel>(listing));
    }

    /// <summary>
    /// Add a new listing.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="listing"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{cryptoId}/listings")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<ListingViewModel>> PostListing(Guid cryptoId, [FromBody] ListingEditViewModel listing)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        // Check if referenced Crypto exists
        if (await _cryptoCurrencyService.GetCryptoCurrencyAsync(cryptoId) == null)
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(Listing.CryptoCurrency),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced cryptocurrency not found."
            });

        _errorManager.ThrowOnErrors();

        var newListing = new Listing()
        {
            CryptoId = cryptoId
        };
        _mapper.Map(listing, newListing);

        await _cryptoCurrencyService.AddListing(newListing);

        return Ok(_mapper.Map<ListingViewModel>(newListing));
    }

    /// <summary>
    /// Modify a listing.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="id"></param>
    /// <param name="listing"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{cryptoId}/listings/{id}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutListing(Guid cryptoId, long id, [FromBody] ListingEditViewModel listing)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedListing = await _cryptoCurrencyService.GetListingById(cryptoId, id);

        if (storedListing == null)
            return NotFound();

        _mapper.Map(listing, storedListing);
        await _cryptoCurrencyService.UpdateListing(storedListing);

        return Ok();
    }

    /// <summary>
    /// Remove a listing.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpDelete]
    [Route("{cryptoId}/listings/{id}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteListing(Guid cryptoId, long id)
    {
        await _cryptoCurrencyService.DeleteListing(cryptoId, id);

        return Ok();
    }

    [HttpPost]
    [Route("/listings/{bookingperiod}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> AddListingTable(string bookingperiod, [FromBody] string data)
    {
        var periodEndDateTime = _bookingPeriodHelper.GetPeriodEndDateTime(bookingperiod);
        var lines = data.Split('\n');

        var headers = lines[0].Split('\t').Select(i => i.Trim().ToUpperInvariant()).ToArray();
        int ticker = Array.IndexOf(headers, "TICKER");
        int usd = Array.IndexOf(headers, "USD");
        int btc = Array.IndexOf(headers, "BTC");
        char decSep = '\0';

        var navListings = await _cryptoCurrencyService.SelectListingsBySource($"NAV {bookingperiod}");

        foreach (var line in lines.Skip(1).ToArray())
        {
            var cols = line.Split('\t');

            if (HasValues(cols, ticker))
            {
                if (decSep.Equals('\0'))
                {
                    // Get the decimal separator from the first BTC value (should be 1.000000)
                    decSep = cols[btc][1].Equals(',') ? ',' : '.';
                }

                var crypto = await _cryptoCurrencyService.GetCryptoCurrencyBySymbol(cols[ticker].Replace("-U", "").Replace("$", ""));

                if (crypto == null) continue;

                var listing = navListings.FirstOrDefault(l => l.CryptoId.Equals(crypto.Id));

                if (listing == null)
                {
                    await _cryptoCurrencyService.AddListing(new()
                    {
                        CryptoId = crypto.Id,
                        BTCPrice = GetDecimal(cols[btc], decSep),
                        USDPrice = GetDecimal(cols[usd], decSep),
                        Source = $"NAV {bookingperiod}",
                        TimeStamp = periodEndDateTime.UtcDateTime
                    });
                }
                else
                {
                    listing.BTCPrice = GetDecimal(cols[btc], decSep);
                    listing.USDPrice = GetDecimal(cols[usd], decSep);
                    listing.TimeStamp = periodEndDateTime.UtcDateTime;

                    await _cryptoCurrencyService.UpdateListing(listing);
                }
            }
        }

        return Ok();

        static bool HasValues(string[] values, int col) =>
            values != null &&
            values.Length > col &&
            !string.IsNullOrWhiteSpace(values[col]) &&
            !values[col].Equals("0");

        static decimal GetDecimal(string value, char decimalSeparator)
        {
            var trimmedValue = value.Replace("$", "").Trim();

            return decimalSeparator != '.'
                ? decimal.Parse(trimmedValue.Replace(".", "").Replace(decimalSeparator, '.'))
                : decimal.Parse(trimmedValue.Replace(",", ""));
        }
    }

    #endregion

    #region External resources

    /// <summary>
    /// Gets the icon for a coin from coinGecko.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns>The png file with logo of the cryptocurrency</returns>
    [HttpGet]
    [Route("icons/{symbol}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCurrencyIcon(string symbol, CancellationToken ct)
    {
        foreach (var marketApi in HttpContext.RequestServices.GetServices<ICryptoMarketApi>())
        {
            try
            {
                var bytes = await marketApi.GetIcon(symbol, ct);

                if (bytes != null)
                    return File(bytes, "image/png");
            }
            catch (NotImplementedException) { }
        }

        return NotFound();
    }

    /// <summary>
    /// This method is meant to retrieve the top n crypto currencies listed by 
    /// the market data providers. It fetches the name, symbol & logo for each 
    /// crypto and then stores them in the CryptoCurrency db table.
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="endIndex"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("import")]
    public async Task<IActionResult> ImportCoins(int startIndex, int endIndex, CancellationToken ct)
    {
        var coins_added = await _cryptoCurrencyService.ImportCoins(startIndex, endIndex, ct);
        return Ok($"Number of crypto currencies added: {coins_added}");
    }

    #endregion
}
