using Hodl.Api.ViewModels.CurrencyModels;

namespace Hodl.Api.Controllers.Currencies;

[ApiController]
[Route("currencies")]
public class CurrencyController : BaseController
{
    private readonly HodlDbContext _db;

    public CurrencyController(
        HodlDbContext dbContext,
        IMapper mapper,
        ILogger<CurrencyController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _db = dbContext;
    }

    #region Currencies
    private async Task<CurrencyRateViewModel> GetCurrencyRate(string isoCode, CancellationToken ct)
    {
        var rate = await _db.CurrencyRates
            .Where(r => r.CurrencyISOCode == isoCode)
            .OrderByDescending(r => r.TimeStamp)
            .FirstOrDefaultAsync(ct);
        return _mapper.Map<CurrencyRateViewModel>(rate);
    }

    /// <summary>
    /// Gets a paged list of all currencies.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<PagingViewModel<CurrencyListViewModel>>> Get(int page, int? itemsPerPage, CancellationToken ct)
    {
        var query = _db.Currencies
            .AsNoTracking()
            .OrderBy(c => c.Name);

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, ct);

        var pageResultView = _mapper.Map<PagingViewModel<CurrencyListViewModel>>(pageResult);
        // And add current/latest listings on the cryptocurrency
        foreach (var currency in pageResultView.Items)
        {
            currency.CurrencyRate = await GetCurrencyRate(currency.ISOCode, ct);
        }

        return Ok(pageResultView);
    }

    /// <summary>
    /// Add a new currency.
    /// </summary>
    /// <param name="currency"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpPost]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<CurrencyListViewModel>> PostCurrency([FromBody] CurrencyEditViewModel currency)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        // Check if key already exists
        if (await _db.Currencies.AnyAsync(c => c.ISOCode == currency.ISOCode))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(currency.ISOCode),
                Code = ErrorCodesStore.DuplicateKey,
                Description = "The currency ISO code already exists."
            });

        _errorManager.ThrowOnErrors();

        var newCurrency = _mapper.Map<Currency>(currency);

        await _db.Currencies.AddAsync(newCurrency);
        await _db.SaveChangesAsync();


        return Ok(_mapper.Map<CurrencyListViewModel>(newCurrency));
    }

    /// <summary>
    /// Gets the selected currency with latest rates.
    /// </summary>
    /// <param name="isoCode"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{isoCode}")]
    public async Task<ActionResult<CurrencyDetailViewModel>> GetCurrency(string isoCode, CancellationToken ct)
    {
        var currency = await _db.Currencies
            .AsNoTracking()
            .Where(c => c.ISOCode == isoCode)
            .Include(c => c.CurrencyRates
                .OrderByDescending(cr => cr.TimeStamp)
                .Take(100))
            .Select(c => _mapper.Map<CurrencyDetailViewModel>(c))
            .SingleOrDefaultAsync(ct);

        if (currency == null)
            return NotFound();

        currency.CurrencyRate = await GetCurrencyRate(currency.ISOCode, ct);

        return Ok(currency);
    }

    /// <summary>
    /// Modify an existing currency.
    /// </summary>
    /// <param name="isoCode"></param>
    /// <param name="currency"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{isoCode}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutCurrency(string isoCode, [FromBody] CurrencyEditViewModel currency)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedCurrency = await _db.Currencies
            .Where(c => c.ISOCode == isoCode)
            .SingleOrDefaultAsync();

        if (storedCurrency == null)
            return NotFound();

        _mapper.Map(currency, storedCurrency);
        await _db.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Remove currency.
    /// </summary>
    /// <param name="isoCode"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("{isoCode}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteCurrency(string isoCode)
    {
        var storedCurrency = await _db.Currencies
            .Where(c => c.ISOCode == isoCode)
            .SingleOrDefaultAsync();

        if (storedCurrency == null)
            return NotFound();

        // TODO: Do checks for existing holdings or NAV's and return nice error messages.

        _db.Currencies.Remove(storedCurrency);
        await _db.SaveChangesAsync();

        return Ok();
    }

    #endregion

    #region CurrencyRates

    /// <summary>
    /// Gets list of all currency rates within a period of time. 
    /// </summary>
    /// <param name="isoCode"></param>
    /// <param name="from">From date given as YYYY-MM-DD or YYYY-MM-DDTHH:mm:ss</param>
    /// <param name="to">From date given as YYYY-MM-DD or YYYY-MM-DDTHH:mm:ss</param>
    /// <returns></returns>
    [HttpGet]
    [Route("{isoCode}/rates")]
    public async Task<ActionResult<PagingViewModel<CurrencyRateViewModel>>> GetCurrencyRates(
        string isoCode, string from, string to, int page, int? itemsPerPage, CancellationToken ct)
    {
        var dateFrom = from.ParamToUtcDate(DateTime.Today.AddYears(-1));
        var dateTo = to.ParamToUtcDate(DateTime.MaxValue);

        var query = _db.CurrencyRates
            .AsNoTracking()
            .Where(r => r.CurrencyISOCode.Equals(isoCode) && r.TimeStamp >= dateFrom && r.TimeStamp <= dateTo)
            .OrderByDescending(r => r.TimeStamp);

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, ct);
        var pageResultView = _mapper.Map<PagingViewModel<CurrencyRateViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    [HttpGet]
    [Route("{isoCode}/rates/{id}")]
    public async Task<ActionResult<CurrencyRateViewModel>> GetCurrencyRates(
        string isoCode, long id, CancellationToken ct)
    {
        var rate = await _db.CurrencyRates
            .Where(r => r.Id == id && r.CurrencyISOCode == isoCode)
            .SingleOrDefaultAsync(ct);

        if (rate == null)
            return NotFound();

        return Ok(_mapper.Map<CurrencyRateViewModel>(rate));
    }

    /// <summary>
    /// Add a new rating for a currency.
    /// </summary>
    /// <param name="isoCode"></param>
    /// <param name="currencyRate"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpPost]
    [Route("{isoCode}/rates")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<CurrencyRateViewModel>> PostCurrencyRate(string isoCode, [FromBody] CurrencyRateEditViewModel currencyRate)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        // Check if referenced Currency exists
        if (await _db.Currencies.SingleOrDefaultAsync(c => c.ISOCode == isoCode) == null)
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(Currency.ISOCode),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced currency not found."
            });

        _errorManager.ThrowOnErrors();

        var newCurrencyRate = new CurrencyRate()
        {
            CurrencyISOCode = isoCode
        };

        _mapper.Map(currencyRate, newCurrencyRate);

        await _db.CurrencyRates.AddAsync(newCurrencyRate);
        await _db.SaveChangesAsync();

        return Ok(_mapper.Map<CurrencyRateViewModel>(newCurrencyRate));
    }

    /// <summary>
    /// Update a rating.
    /// </summary>
    /// <param name="isoCode"></param>
    /// <param name="id"></param>
    /// <param name="currencyRate"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{isoCode}/rates/{id}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutCurrencyRate(string isoCode, long id, [FromBody] CurrencyRateEditViewModel currencyRate)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedCurrencyRate = await _db.CurrencyRates
            .Where(r => r.Id == id)
            .SingleOrDefaultAsync();

        if (storedCurrencyRate == null)
            return NotFound();

        if (storedCurrencyRate.CurrencyISOCode != isoCode)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(CurrencyRate.CurrencyISOCode),
                Code = ErrorCodesStore.ReferenceCheckFailed,
                Description = "The currency code for the rating does not match."
            });

        _errorManager.ThrowOnErrors();

        _mapper.Map(currencyRate, storedCurrencyRate);
        await _db.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Removes the currency rating.
    /// </summary>
    /// <param name="isoCode"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("{isoCode}/rates/{id}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteCurrencyRate(string isoCode, long id)
    {
        var storedCurrencyRate = await _db.CurrencyRates
            .Where(r => r.Id == id)
            .SingleOrDefaultAsync();

        if (storedCurrencyRate == null)
            return NotFound();

        if (storedCurrencyRate.CurrencyISOCode != isoCode)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(CurrencyRate.CurrencyISOCode),
                Code = ErrorCodesStore.ReferenceCheckFailed,
                Description = "The currency code for the rating does not match."
            });

        // TODO: Do checks for existing holdings or NAV's and return nice error messages.

        _errorManager.ThrowOnErrors();

        _db.CurrencyRates.Remove(storedCurrencyRate);
        await _db.SaveChangesAsync();

        return Ok();
    }

    #endregion
}
