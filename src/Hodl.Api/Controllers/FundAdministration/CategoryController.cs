using Hodl.Api.FilterParams;
using Hodl.Api.ViewModels.CurrencyModels;
using Hodl.Api.ViewModels.FundModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hodl.Api.Controllers.FundAdministration;

[ApiController]
[Route("categories")]
public class CategoryController : BaseController
{
    private readonly HodlDbContext _db;
    private readonly IChangeLogService _changeLogService;

    public CategoryController(
        HodlDbContext dbContext,
        IChangeLogService changeLogService,
        IMapper mapper,
        ILogger<CategoryController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _db = dbContext;
        _changeLogService = changeLogService;
    }

    #region Categories

    [HttpGet]
    public async Task<ActionResult<PagingViewModel<CategoryViewModel>>> GetCategories(
        string group, int page, int? itemsPerPage, CancellationToken ct)
    {
        // Filters
        var filter = new CategoryFilterParams
        {
            Group = group
        };

        var query = _db.Categories
            .AsNoTracking()
            .Filter(filter)
            .OrderBy(c => c.Name);

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, ct);
        var pageResultView = _mapper.Map<PagingViewModel<CategoryViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    [HttpGet]
    [Route("groups")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategoryGroups(CancellationToken ct)
    {
        var groups = await _db.Categories
            .AsNoTracking()
            .Select(c => c.Group)
            .Distinct()
            .OrderBy(g => g)
            .ToArrayAsync(ct);

        return Ok(groups);
    }

    [HttpGet]
    [Route("{categoryId}")]
    public async Task<ActionResult<CategoryViewModel>> GetCategory(Guid categoryId, CancellationToken ct)
    {
        var category = await _db.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == categoryId, ct);

        if (category == null)
            return NotFound();

        return Ok(_mapper.Map<CategoryViewModel>(category));
    }

    [HttpPost]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader,Trader")]
#endif
    public async Task<ActionResult<CategoryViewModel>> PostCategory([FromBody] CategoryEditViewModel category)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var newCategory = _mapper.Map<Category>(category);

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("Categories", null, newCategory);
            await _db.Categories.AddAsync(newCategory);
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

        return Ok(_mapper.Map<CategoryViewModel>(newCategory));
    }

    [HttpPut]
    [Route("{categoryId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader,Trader")]
#endif
    public async Task<IActionResult> PutCategory(Guid categoryId, [FromBody] CategoryEditViewModel category)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedCategory = await _db.Categories
            .SingleOrDefaultAsync(c => c.Id == categoryId);

        if (storedCategory == null)
        {
            return NotFound();
        }

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("Categories", storedCategory, category);
            _mapper.Map(category, storedCategory);
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

    [HttpDelete]
    [Route("{categoryId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader,Trader")]
#endif
    public async Task<IActionResult> DeleteCategory(Guid categoryId)
    {
        var storedCategory = await _db.Categories
            .SingleOrDefaultAsync(c => c.Id == categoryId);

        if (storedCategory == null)
        {
            return NotFound();
        }

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("Categories", storedCategory, null);
            _db.Categories.Remove(storedCategory);
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

    [HttpGet]
    [Route("{categoryId}/cryptos")]
    public async Task<ActionResult<PagingViewModel<CryptoCurrencyListViewModel>>> CategoryGetCryptos(
        Guid categoryId, int page, int? itemsPerPage, CancellationToken ct)
    {
        if (!await _db.Categories.AnyAsync(c => c.Id == categoryId, ct))
            _errorManager.AddValidationError(HttpStatusCode.NotFound, "Category not found", "CategoryId");

        _errorManager.ThrowOnErrors();

        var query = _db.CryptoCategories
            .Where(c => c.CategoryId == categoryId)
            .Include(c => c.CryptoCurrency)
            .Select(c => c.CryptoCurrency)
            .OrderBy(c => c.Name);

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, ct);
        var pageResultView = _mapper.Map<PagingViewModel<CryptoCurrencyListViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    [HttpPost]
    [Route("{categoryId}/cryptos/{cryptoId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader,Trader")]
#endif
    public async Task<IActionResult> CategoryAddCrypto(Guid categoryId, Guid cryptoId)
    {
        if (!await _db.Categories.AnyAsync(c => c.Id == categoryId))
            _errorManager.AddValidationError(HttpStatusCode.NotFound, "Category not found", "CategoryId");

        if (!await _db.Categories.AnyAsync(c => c.Id == categoryId))
            _errorManager.AddValidationError(HttpStatusCode.NotFound, "Crypto currency not found", "CryptoId");

        if (await _db.CryptoCategories.AnyAsync(c => c.CategoryId == categoryId && c.CryptoId == cryptoId))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, "Crypto currency is already added to the category", "CategoryId, CryptoId");

        _errorManager.ThrowOnErrors();

        var newCryptoCategory = new CryptoCategory()
        {
            CategoryId = categoryId,
            CryptoId = cryptoId
        };

        await _changeLogService.AddChangeLogAsync("CryptoCategories", null, newCryptoCategory);
        _db.CryptoCategories.Add(newCryptoCategory);
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete]
    [Route("{categoryId}/cryptos/{cryptoId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader,Trader")]
#endif
    public async Task<IActionResult> CategoryDelCrypto(Guid categoryId, Guid cryptoId)
    {
        var storedCryptoCategory = await _db.CryptoCategories
            .SingleOrDefaultAsync(c => c.CategoryId == categoryId && c.CryptoId == cryptoId);

        if (storedCryptoCategory == null)
        {
            return NotFound();
        }

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("CryptoCategories", storedCryptoCategory, null);
            _db.CryptoCategories.Remove(storedCryptoCategory);
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
