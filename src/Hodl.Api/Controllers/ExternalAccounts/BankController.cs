using Hodl.Api.FilterParams;
using Hodl.Api.ViewModels.ExternalAccountModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hodl.Api.Controllers.ExternalAccounts;

[ApiController]
[Route("banks")]
public class BankController : BaseController
{
    private readonly HodlDbContext _db;
    private readonly IChangeLogService _changeLogService;

    public BankController(
        HodlDbContext dbContext,
        IChangeLogService changeLogService,
        IMapper mapper,
        ILogger<BankController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _db = dbContext;
        _changeLogService = changeLogService;
    }

    #region Banks

    /// <summary>
    /// Gets a paged list of all banks.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <returns></returns>
    [HttpGet]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<PagingViewModel<BankListViewModel>>> Get(int page, int? itemsPerPage, CancellationToken ct)
    {
        var query = _db.Banks
            .AsNoTracking()
            .OrderBy(b => b.Name);

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, ct);

        var pageResultView = _mapper.Map<PagingViewModel<BankListViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    /// <summary>
    /// Add a new Bank.
    /// </summary>
    /// <param name="bank"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpPost]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<BankListViewModel>> PostBank([FromBody] BankEditViewModel bank)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        if (await _db.Banks.AnyAsync(b => b.BIC == bank.BIC))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Bank.BIC),
                Code = ErrorCodesStore.DuplicateKey,
                Description = "The bank code is already in use."
            });

        _errorManager.ThrowOnErrors();

        var newBank = _mapper.Map<Bank>(bank);

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("Banks", null, newBank);
            await _db.Banks.AddAsync(newBank);
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

        return Ok(_mapper.Map<BankListViewModel>(newBank));
    }

    /// <summary>
    /// Gets the selected Bank with latest rates.
    /// </summary>
    /// <param name="bankId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{bankId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<BankDetailViewModel>> GetBank(Guid bankId, CancellationToken ct)
    {
        var bank = await _db.Banks
            .AsNoTracking()
            .Where(b => b.Id == bankId)
            .Include(b => b.BankAccounts
                .OrderBy(ba => ba.HolderName)
                .Take(PagingConstants.DEFAULT_ITEMS_PER_PAGE))
            .SingleOrDefaultAsync(ct);

        if (bank == null)
            return NotFound();

        return Ok(_mapper.Map<BankDetailViewModel>(bank));
    }

    /// <summary>
    /// Modify an existing Bank.
    /// </summary>
    /// <param name="bankId"></param>
    /// <param name="bank"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{bankId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutBank(Guid bankId, [FromBody] BankEditViewModel bank)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedBank = await _db.Banks
            .Where(b => b.Id == bankId)
            .SingleOrDefaultAsync();

        if (storedBank == null)
            return NotFound();

        if (await _db.Banks.AnyAsync(b => b.BIC == bank.BIC && b.Id != bankId))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Bank.BIC),
                Code = ErrorCodesStore.DuplicateKey,
                Description = "The bank code is already in use."
            });

        _errorManager.ThrowOnErrors();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("Banks", storedBank, bank);
            _mapper.Map(bank, storedBank);
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

        return Ok();
    }

    /// <summary>
    /// Remove Bank.
    /// </summary>
    /// <param name="bankId"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("{bankId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteBank(Guid bankId)
    {
        var storedBank = await _db.Banks
            .Where(b => b.Id == bankId)
            .Include(b => b.BankAccounts)
            .AsSplitQuery()
            .SingleOrDefaultAsync();

        if (storedBank == null)
            return NotFound();

        // Do checks for existing accounts.
        if (storedBank.BankAccounts.Any())
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Bank.Id),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "Bank has accounts linked so can not be deleted."
            });

        _errorManager.ThrowOnErrors();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("Banks", storedBank, null);
            _db.Banks.Remove(storedBank);
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

        return Ok();
    }

    #endregion

    #region BankAccounts

    /// <summary>
    /// Gets a paged list of all bankaccounts for a bank.
    /// </summary>
    /// <param name="bankId"></param>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{bankId}/accounts")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<PagingViewModel<BankAccountListViewModel>>> GetBankAccountsByBank(
        Guid bankId, int page, int? itemsPerPage, CancellationToken ct)
    {
        var query = _db.BankAccounts
            .AsNoTracking()
            .Where(a => a.BankId.Equals(bankId))
            .Include(a => a.Bank)
            .Include(a => a.Fund)
            .OrderBy(a => a.HolderName)
            .ThenBy(a => a.IBAN)
            .AsSplitQuery();

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, ct);

        var pageResultView = _mapper.Map<PagingViewModel<BankAccountListViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    /// <summary>
    /// Gets a paged list of all bankaccounts.
    /// </summary>
    /// <param name="bankId"></param>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("/bankaccounts")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<PagingViewModel<BankAccountListViewModel>>> GetBankAccounts(
        Guid? fundId, int page, int? itemsPerPage, CancellationToken ct)
    {
        // you can skip properies here
        var filter = new BankAccountFilterParams
        {
            FundId = fundId,
        };

        var query = _db.BankAccounts
            .AsNoTracking()
            .Filter(filter)
            .Include(a => a.Bank)
            .Include(a => a.Fund)
            .OrderBy(a => a.HolderName)
            .ThenBy(a => a.IBAN)
            .AsSplitQuery();

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, ct);
        var pageResultView = _mapper.Map<PagingViewModel<BankAccountListViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    /// <summary>
    /// Gets the selected BankAccount.
    /// </summary>
    /// <param name="bankAccountId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("/bankaccounts/{bankAccountId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<BankAccountDetailViewModel>> GetBankAccount(Guid bankAccountId, CancellationToken ct)
    {
        var bankAccount = await _db.BankAccounts
            .AsNoTracking()
            .Where(a => a.Id == bankAccountId)
            .Include(a => a.Bank)
            .Include(a => a.Fund)
            .Include(a => a.Balances)
            .AsSplitQuery()
            .SingleOrDefaultAsync(ct);

        if (bankAccount == null)
            return NotFound();

        return Ok(_mapper.Map<BankAccountDetailViewModel>(bankAccount));
    }

    /// <summary>
    /// Add a new bankAccount.
    /// </summary>
    /// <param name="bankAccount"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpPost]
    [Route("/bankaccounts")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<BankAccountListViewModel>> PostBankAccount([FromBody] BankAccountEditViewModel bankAccount)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        // Check if referenced Bank exists
        if (await _db.Banks.SingleOrDefaultAsync(b => b.Id == bankAccount.BankId) == null)
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(BankAccount.Bank),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced bank not found."
            });
        // Check if referenced Fund exists
        if (await _db.Funds.SingleOrDefaultAsync(f => f.Id == bankAccount.FundId) == null)
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(BankAccount.Fund),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced fund not found."
            });
        // Check if IBAN is unique
        if (await _db.BankAccounts.AnyAsync(a => a.IBAN == bankAccount.IBAN))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Bank.BIC),
                Code = ErrorCodesStore.DuplicateKey,
                Description = "The bank account number (IBAN) is already in use."
            });

        _errorManager.ThrowOnErrors();

        var newBankAccount = new BankAccount();

        _mapper.Map(bankAccount, newBankAccount);

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("BankAccounts", null, newBankAccount);
            await _db.BankAccounts.AddAsync(newBankAccount);
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

        return Ok(_mapper.Map<BankAccountListViewModel>(newBankAccount));
    }

    /// <summary>
    /// Update a rating.
    /// </summary>
    /// <param name="bankAccountId"></param>
    /// <param name="id"></param>
    /// <param name="bankAccount"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("/bankaccounts/{bankAccountId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutBankAccount(Guid bankAccountId, [FromBody] BankAccountEditViewModel bankAccount)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedBankAccount = await _db.BankAccounts
            .Where(a => a.Id == bankAccountId)
            .SingleOrDefaultAsync();

        if (storedBankAccount == null)
            return NotFound();

        // Check if referenced Bank exists
        if (await _db.Banks.SingleOrDefaultAsync(b => b.Id == bankAccount.BankId) == null)
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(BankAccount.Bank),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced bank not found."
            });
        // Check if referenced Fund exists
        if (await _db.Funds.SingleOrDefaultAsync(f => f.Id == bankAccount.FundId) == null)
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(BankAccount.Fund),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced fund not found."
            });
        // Check if IBAN is unique
        if (await _db.BankAccounts.AnyAsync(a => a.IBAN == bankAccount.IBAN && a.Id != bankAccountId))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Bank.BIC),
                Code = ErrorCodesStore.DuplicateKey,
                Description = "The bank account number (IBAN) is already in use."
            });

        _errorManager.ThrowOnErrors();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("BankAccounts", storedBankAccount, bankAccount);
            _mapper.Map(bankAccount, storedBankAccount);
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

        return Ok();
    }

    /// <summary>
    /// Removes the Bank Account.
    /// </summary>
    /// <param name="bankAccountId"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("/bankaccounts/{bankAccountId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteBankAccount(Guid bankAccountId)
    {
        var storedBankAccount = await _db.BankAccounts
            .Where(a => a.Id == bankAccountId)
            .SingleOrDefaultAsync();

        if (storedBankAccount == null)
            return NotFound();

        // Do checks for existing balances and return nice error messages.
        if (await _db.BankBalances.AnyAsync(t => t.BankAccountId == bankAccountId))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(BankAccount.Balances),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "Bankaccount has balances linked so can not be deleted."
            });

        _errorManager.ThrowOnErrors();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("BankAccounts", storedBankAccount, null);
            _db.BankAccounts.Remove(storedBankAccount);
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

        return Ok();
    }

    #endregion

    #region BankBalances

    /// <summary>
    /// Gets all the balances of a bank account.
    /// </summary>
    /// <param name="bankAccountId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("/bankaccounts/{bankAccountId}/balances")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<IEnumerable<BankBalanceListViewModel>>> GetBankBalances(Guid bankAccountId, CancellationToken ct)
    {
        var balances = await _db.BankBalances
            .AsNoTracking()
            .Where(b => b.BankAccountId == bankAccountId)
            .Include(b => b.Currency)
            .OrderBy(b => b.CurrencyISOCode)
            .Select(b => _mapper.Map<BankBalanceListViewModel>(b))
            .ToListAsync(ct);

        return Ok(balances);
    }

    /// <summary>
    /// Gets the selected BankBalance.
    /// </summary>
    /// <param name="bankBalanceId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("/bankaccounts/{bankAccountId}/balances/{bankBalanceId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<BankBalanceDetailViewModel>> GetBankBalance(Guid bankAccountId, Guid bankBalanceId, CancellationToken ct)
    {
        var bankBalance = await _db.BankBalances
            .AsNoTracking()
            .Where(b => b.Id == bankBalanceId)
            .Include(b => b.BankAccount)
            .ThenInclude(a => a.Bank)
            .Include(b => b.Currency)
            .AsSplitQuery()
            .SingleOrDefaultAsync(ct);

        if (bankBalance == null)
            return NotFound();

        if (bankBalance.BankAccountId != bankAccountId)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(BankBalance.BankAccountId),
                Code = ErrorCodesStore.ReferenceCheckFailed,
                Description = "The bank account for the balance does not match."
            });

        _errorManager.ThrowOnErrors();

        return Ok(_mapper.Map<BankBalanceDetailViewModel>(bankBalance));
    }

    /// <summary>
    /// Add a new bankAccount.
    /// </summary>
    /// <param name="bankAccountId"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("/bankaccounts/{bankAccountId}/balances")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<BankBalanceListViewModel>> PostBankBalance(Guid bankAccountId, [FromBody] BankBalanceAddViewModel bankBalance)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        // Check if referenced BankAccount exists
        if (await _db.BankAccounts.SingleOrDefaultAsync(a => a.Id == bankAccountId) == null)
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(BankBalance.BankAccount),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced bank account not found."
            });
        // Check if referenced Currencies exists
        if (await _db.Currencies.SingleOrDefaultAsync(c => c.ISOCode == bankBalance.CurrencyISOCode) == null)
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(BankBalance.Currency),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced currency not found."
            });
        // Check if currency balance for the account is unique
        if (await _db.BankBalances.AnyAsync(b => b.BankAccountId == bankAccountId && b.CurrencyISOCode == bankBalance.CurrencyISOCode))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = "BankAccount, CurrencyISOCode",
                Code = ErrorCodesStore.DuplicateKey,
                Description = "The bank account already has a balance for the given currency."
            });

        _errorManager.ThrowOnErrors();

        var newBankBalance = new BankBalance();

        _mapper.Map(bankBalance, newBankBalance);

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("BankBalances", null, newBankBalance);
            await _db.BankBalances.AddAsync(newBankBalance);
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

        return Ok(_mapper.Map<BankBalanceListViewModel>(newBankBalance));
    }

    /// <summary>
    /// Update a rating.
    /// </summary>
    /// <param name="bankAccountId"></param>
    /// <param name="bankBalanceId"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("/bankaccounts/{bankAccountId}/balances/{bankBalanceId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutBankBalance(Guid _, Guid bankBalanceId, [FromBody] BankBalanceEditViewModel bankBalance)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedBankBalance = await _db.BankBalances
            .Where(b => b.Id == bankBalanceId)
            .SingleOrDefaultAsync();

        if (storedBankBalance == null)
            return NotFound();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("BankBalances", storedBankBalance, bankBalance);
            _mapper.Map(bankBalance, storedBankBalance);
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

        return Ok();
    }

    /// <summary>
    /// Removes the Bank Account.
    /// </summary>
    /// <param name="bankAccountId"></param>
    /// <param name="bankBalanceId"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("/bankaccounts/{bankAccountId}/balances/{bankBalanceId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteBankBalance(Guid bankAccountId, Guid bankBalanceId)
    {
        var storedBankBalance = await _db.BankBalances
            .Where(b => b.Id == bankBalanceId)
            .FirstOrDefaultAsync();

        if (storedBankBalance == null)
            return NotFound();

        if (storedBankBalance.BankAccountId != bankAccountId)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(BankBalance.BankAccountId),
                Code = ErrorCodesStore.ReferenceCheckFailed,
                Description = "The bank account for the balance does not match."
            });

        _errorManager.ThrowOnErrors();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("BankBalances", storedBankBalance, null);
            _db.BankBalances.Remove(storedBankBalance);
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

        return Ok();
    }

    #endregion
}
