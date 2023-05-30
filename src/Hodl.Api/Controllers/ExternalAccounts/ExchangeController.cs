using Hodl.Api.ViewModels.ExternalAccountModels;
using Microsoft.Extensions.Options;

namespace Hodl.Api.Controllers.ExternalAccounts;

[ApiController]
[Route("exchanges")]
public class ExchangeController : BaseController
{
    private readonly HodlDbContext _db;
    private readonly IExchangeAccountsService _exchangeAccountsService;

    private readonly bool _isTestEnvironment = true;

    public ExchangeController(
        IOptions<AppDefaults> settings,
        HodlDbContext dbContext,
        IExchangeAccountsService exchangeAccountsService,
        IMapper mapper,
        ILogger<ExchangeController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _db = dbContext;
        _exchangeAccountsService = exchangeAccountsService;

        _isTestEnvironment = settings.Value.IsTestEnvironment();
    }

    #region Exchanges

    /// <summary>
    /// Gets a paged list of all exchanges.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<PagingViewModel<ExchangeListViewModel>>> Get(int page, int? itemsPerPage, CancellationToken ct)
    {
        var query = _db.Exchanges
            .AsNoTracking()
            .OrderBy(e => e.ExchangeName);

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, ct);

        var pageResultView = _mapper.Map<PagingViewModel<ExchangeListViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    /// <summary>
    /// Add a new Exchange.
    /// </summary>
    /// <param name="exchange"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpPost]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<ExchangeListViewModel>> PostExchange([FromBody] ExchangeEditViewModel exchange)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var newExchange = _mapper.Map<Exchange>(exchange);

        await _db.Exchanges.AddAsync(newExchange);
        await _db.SaveChangesAsync();

        return Ok(_mapper.Map<ExchangeListViewModel>(newExchange));
    }

    /// <summary>
    /// Gets the selected Exchange with latest rates.
    /// </summary>
    /// <param name="exchangeId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{exchangeId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<ExchangeDetailViewModel>> GetExchange(Guid exchangeId, CancellationToken ct)
    {
        var exchange = await _db.Exchanges
            .AsNoTracking()
            .Where(e => e.Id == exchangeId)
            .Include(e => e.ExchangeAccounts
                .OrderBy(ea => ea.Name)
                .Take(PagingConstants.DEFAULT_ITEMS_PER_PAGE))
            .SingleOrDefaultAsync(ct);

        if (exchange == null)
            return NotFound();

        return Ok(_mapper.Map<ExchangeDetailViewModel>(exchange));
    }

    /// <summary>
    /// Gets the exchanges icon as png image.
    /// </summary>
    /// <param name="exchangeId"></param>
    /// <returns>The icon of the exchange associated with the provided ID</returns>
    [HttpGet]
    [Route("{exchangeId}/icon"), ResponseCache(Duration = 3600)]
    [AllowAnonymous]
    public async Task<IActionResult> GetExchangeIcon(Guid exchangeId, CancellationToken ct)
    {
        // Get the bytes from the DB record
        var exchange = await _db.Exchanges
            .Where(c => c.Id == exchangeId)
            .SingleOrDefaultAsync(ct);

        if (exchange == null || exchange.Icon == null || exchange.Icon.Length == 0)
            return NotFound();

        return File(exchange.Icon, "image/png");
    }

    /// <summary>
    /// Modify an existing Exchange.
    /// </summary>
    /// <param name="exchangeId"></param>
    /// <param name="exchange"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{exchangeId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutExchange(Guid exchangeId, [FromBody] ExchangeEditViewModel exchange)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedExchange = await _db.Exchanges
            .Where(e => e.Id == exchangeId)
            .SingleOrDefaultAsync();

        if (storedExchange == null)
            return NotFound();

        _mapper.Map(exchange, storedExchange);
        await _db.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Remove Exchange.
    /// </summary>
    /// <param name="exchangeId"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("{exchangeId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteExchange(Guid exchangeId)
    {
        var storedExchange = await _db.Exchanges
            .Where(e => e.Id == exchangeId)
            .Include(e => e.ExchangeAccounts)
            .AsSplitQuery()
            .SingleOrDefaultAsync();

        if (storedExchange == null)
            return NotFound();

        // Do checks for existing accounts.
        if (storedExchange.ExchangeAccounts.Any())
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Exchange.ExchangeAccounts),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "Exchange has accounts linked so can not be deleted."
            });

        _errorManager.ThrowOnErrors();

        _db.Exchanges.Remove(storedExchange);
        await _db.SaveChangesAsync();

        return Ok();
    }

    #endregion

    #region ExchangeAccounts

    /// <summary>
    /// Gets a paged list of all exchangeaccounts.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("/exchangeaccounts")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<PagingViewModel<ExchangeAccountListViewModel>>> GetExchangeAccounts(
        int page, int? itemsPerPage, CancellationToken ct)
    {
        var query = _db.ExchangeAccounts
            .AsNoTracking()
            .Include(ea => ea.Exchange)
            .OrderBy(ea => ea.Name)
            .AsSingleQuery();

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, ct);

        var pageResultView = _mapper.Map<PagingViewModel<ExchangeAccountListViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    /// <summary>
    /// Gets a paged list of all exchangeaccounts for an exchange.
    /// </summary>
    /// <param name="exchangeId"></param>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{exchangeId}/accounts")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<PagingViewModel<ExchangeAccountListViewModel>>> GetExchangeAccountsByExchange(
        Guid exchangeId, int page, int? itemsPerPage, CancellationToken ct)
    {
        var query = _db.ExchangeAccounts
            .AsNoTracking()
            .Where(ea => ea.ExchangeId.Equals(exchangeId))
            .Include(ea => ea.Exchange)
            .OrderBy(ea => ea.Name)
            .AsSingleQuery();

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, ct);

        var pageResultView = _mapper.Map<PagingViewModel<ExchangeAccountListViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    /// <summary>
    /// Gets the selected ExchangeAccount.
    /// </summary>
    /// <param name="exchangeAccountId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("/exchangeaccounts/{exchangeAccountId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<ExchangeDetailViewModel>> GetExchangeAccount(Guid exchangeAccountId, CancellationToken ct)
    {
        var exchangeAccount = await _exchangeAccountsService.GetExchangeAccount(exchangeAccountId, ct);

        if (exchangeAccount == null)
            return NotFound();

        return Ok(_mapper.Map<ExchangeAccountDetailViewModel>(exchangeAccount));
    }

    /// <summary>
    /// Add a new exchangeAccount.
    /// </summary>
    /// <param name="exchangeAccount"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpPost]
    [Route("/exchangeaccounts")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<ExchangeAccountListViewModel>> PostExchangeAccount([FromBody] ExchangeAccountEditViewModel exchangeAccount)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        // Check if referenced Exchange exists
        if (await _db.Exchanges.SingleOrDefaultAsync(e => e.Id == exchangeAccount.ExchangeId) == null)
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(ExchangeAccount.Exchange),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced exchange not found."
            });
        // Check if no double account keys are stored
        if (await _db.ExchangeAccounts.AnyAsync(a => a.ExchangeId == exchangeAccount.ExchangeId && a.AccountKey.Equals(exchangeAccount.AccountKey)))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(ExchangeAccount.AccountKey),
                Code = ErrorCodesStore.DuplicateKey,
                Description = "The account key is already registered on the exchange."
            });
        // If parent reference is set, check the existance
        if (exchangeAccount.ParentAccountId != null &&
            await _db.ExchangeAccounts.SingleOrDefaultAsync(ea => ea.ParentAccountId == exchangeAccount.ParentAccountId) == null)
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(ExchangeAccount.ParentAccount),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced parent account not found."
            });

        _errorManager.ThrowOnErrors();

        var newExchangeAccount = new ExchangeAccount();

        _mapper.Map(exchangeAccount, newExchangeAccount);

        await _db.ExchangeAccounts.AddAsync(newExchangeAccount);
        await _db.SaveChangesAsync();

        return Ok(_mapper.Map<ExchangeAccountListViewModel>(newExchangeAccount));
    }

    /// <summary>
    /// Update a rating.
    /// </summary>
    /// <param name="exchangeAccountId"></param>
    /// <param name="id"></param>
    /// <param name="exchangeAccount"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("/exchangeaccounts/{exchangeAccountId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutExchangeAccount(Guid exchangeAccountId, [FromBody] ExchangeAccountEditViewModel exchangeAccount)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedExchangeAccount = await _db.ExchangeAccounts
            .Where(ea => ea.Id == exchangeAccountId)
            .SingleOrDefaultAsync();

        if (storedExchangeAccount == null)
            return NotFound();

        // Check if no double account keys are stored
        if (await _db.ExchangeAccounts.AnyAsync(a => a.ExchangeId == exchangeAccount.ExchangeId && a.AccountKey == exchangeAccount.AccountKey && a.Id != exchangeAccountId))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(ExchangeAccount.AccountKey),
                Code = ErrorCodesStore.DuplicateKey,
                Description = "The account key is already registered on the exchange."
            });
        // If parent reference is set, check the existance
        if (exchangeAccount.ParentAccountId != null &&
            await _db.ExchangeAccounts.SingleOrDefaultAsync(ea => ea.ParentAccountId == exchangeAccount.ParentAccountId) == null)
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(ExchangeAccount.ParentAccount),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced parent account not found."
            });

        _errorManager.ThrowOnErrors();

        _mapper.Map(exchangeAccount, storedExchangeAccount);
        await _db.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Removes the Exchange rating.
    /// </summary>
    /// <param name="exchangeId"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("/exchangeaccounts/{exchangeAccountId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteExchangeAccount(Guid exchangeAccountId)
    {
        var storedExchangeAccount = await _db.ExchangeAccounts
            .Where(ea => ea.Id == exchangeAccountId)
            .SingleOrDefaultAsync();

        if (storedExchangeAccount == null)
            return NotFound();

        // TODO: Do checks for existing holdings or NAV's and return nice error messages.
        if (await _db.Orders.AnyAsync(t => t.ExchangeAccountId == exchangeAccountId))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(ExchangeAccount.Id),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "Exchangeaccount has trades linked so can not be deleted."
            });

        _errorManager.ThrowOnErrors();

        _db.ExchangeAccounts.Remove(storedExchangeAccount);
        await _db.SaveChangesAsync();

        return Ok();
    }

    #endregion

    #region ExchangeAccount balances

    [HttpGet]
    [Route("/exchangeaccounts/{exchangeAccountId}/balances")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<IEnumerable<WalletBalanceListViewModel>>> GetExchangeAccountBalances(Guid exchangeAccountId, CancellationToken ct)
    {
        var balances = await _exchangeAccountsService.GetExchangeAccountBalances(exchangeAccountId, ct);

        return Ok(balances
            .Select(b => _mapper.Map<WalletBalanceListViewModel>(b))
            .ToArray());
    }

    [HttpGet]
    [Route("/exchangeaccounts/{exchangeAccountId}/balances/update")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> UpdateExchangeAccountBalances(Guid exchangeAccountId)
    {
        await _exchangeAccountsService.UpdateExchangeAccountBalances(exchangeAccountId, _isTestEnvironment);

        return Ok();
    }

    #endregion
}
