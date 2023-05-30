using Hodl.Api.ViewModels.FundModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hodl.Api.Controllers.FundAdministration;

[ApiController]
[Route("funds")]
public class FundController : BaseController
{
    private readonly HodlDbContext _db;
    private readonly IChangeLogService _changeLogService;
    private readonly IBookingPeriodHelper _bookingPeriodHelper;
    private readonly IFundService _fundService;
    private readonly ICurrencyService _currencyService;
    private readonly ICryptoCurrencyService _cryptoService;
    private readonly ILayerIdxService _layerIdxService;

    public FundController(
        HodlDbContext dbContext,
        IChangeLogService changeLogService,
        ILayerIdxService layerIdxService,
        IBookingPeriodHelper bookingPeriodHelper,
        IFundService fundService,
        ICurrencyService currencyService,
        ICryptoCurrencyService cryptoCurrencyService,
        IMapper mapper,
        ILogger<FundController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _db = dbContext;
        _changeLogService = changeLogService;
        _layerIdxService = layerIdxService;
        _bookingPeriodHelper = bookingPeriodHelper;
        _fundService = fundService;
        _currencyService = currencyService;
        _cryptoService = cryptoCurrencyService;
    }

    private async Task<PeriodNavViewModel> GetCurrentNavViewModel(Guid fundId, CancellationToken ct) =>
        _mapper.Map<PeriodNavViewModel>(await _fundService.GetCurrentNav(fundId, ct));

    #region Fund

    /// <summary>
    /// Gets a paged list of funds with overview properties.
    /// </summary>
    /// <param name="page">Page index (optional)</param>
    /// <param name="itemsPerPage">Number of items per page (optional, default 20)</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<PagingViewModel<FundListViewModel>>> Get(int page, int? itemsPerPage, CancellationToken ct)
    {
        // Create a paged resultset
        var pageResult = await _fundService.GetFunds(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, ct);
        var pageResultView = _mapper.Map<PagingViewModel<FundListViewModel>>(pageResult);

        // And add current/latest NAV to the fund
        foreach (var fund in pageResultView.Items)
        {
            fund.Nav = await GetCurrentNavViewModel(fund.Id, ct);
            fund.CurrentBookingPeriod = await _fundService.CurrentBookingPeriod(fund.Id, ct);
        }

        return Ok(pageResultView);
    }

    [HttpGet]
    [Route("{fundId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader,Trader")]
#endif
    public async Task<ActionResult<FundDetailViewModel>> GetFund(Guid fundId, CancellationToken ct)
    {
        var fund = await _fundService.GetFundDetailed(fundId, ct);

        if (fund == null)
            return NotFound();

        var fundModel = _mapper.Map<FundDetailViewModel>(fund);

        var actualBookingPeriod = _bookingPeriodHelper.CalcBookingPeriod(DateTimeOffset.UtcNow);

        fundModel.Nav = await GetCurrentNavViewModel(fund.Id, ct);
        fundModel.CurrentBookingPeriod = await _fundService.CurrentBookingPeriod(fundId, ct);
        fundModel.CloseHistoryButton = string.Compare(fundModel.CurrentBookingPeriod, actualBookingPeriod) < 0;

        // TODO: Get full fund overview including categories and fund layers distributions

        return Ok(fundModel);
    }

    /// <summary>
    /// Add a new fund.
    /// </summary>
    /// <param name="fund"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpPost]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<FundListViewModel>> PostFund([FromBody] FundEditViewModel fund)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var newFund = _mapper.Map<Fund>(fund);

        await _fundService.AddFund(newFund);

        return Ok(_mapper.Map<FundListViewModel>(newFund));
    }

    [HttpPut]
    [Route("{fundId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutFund(Guid fundId, [FromBody] FundEditViewModel fund)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedFund = await _fundService.GetFund(fundId);

        if (storedFund == null)
            return NotFound();

        // Apply and save the changes
        _mapper.Map(fund, storedFund);

        await _fundService.UpdateFund(storedFund);

        return Ok();
    }

    [HttpDelete]
    [Route("{fundId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteFund(Guid fundId)
    {
        var storedFund = await _fundService.GetFund(fundId);

        if (storedFund == null)
            return NotFound();

        await _fundService.DeleteFund(fundId);

        return Ok();
    }

    /// <summary>
    /// Gets all the available booking periods for a fund.
    /// </summary>
    /// <param name="fundId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{fundId}/bookingperiods")]
    public async Task<ActionResult<string[]>> GetAllBookingPeriods(Guid fundId, CancellationToken ct) =>
        Ok(await _fundService.GetAllBookingPeriods(fundId, ct));

    #endregion

    #region FundOwner

    /// <summary>
    /// Gets a paged list of fund owners with overview properties.
    /// </summary>
    /// <param name="page">Page index (optional)</param>
    /// <param name="itemsPerPage">Number of items per page (optional, default 20)</param>
    /// <returns></returns>
    [HttpGet]
    [Route("owner")]
    public async Task<ActionResult<PagingViewModel<FundOwnerListViewModel>>> GetFundOwners(int page, int? itemsPerPage, CancellationToken ct)
    {
        var pageResult = await _fundService.GetFundOwners(page, itemsPerPage, ct);
        var pageResultView = _mapper.Map<PagingViewModel<FundOwnerListViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    /// <summary>
    /// Add a new fund.
    /// </summary>
    /// <param name="fundOwner"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpPost]
    [Route("owner")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<FundOwnerListViewModel>> PostFundOwner([FromBody] FundOwnerEditViewModel fundOwner)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        // Check if unique condition is matched: nameof(Symbol), nameof(Name)
        if (await _db.FundOwners.AnyAsync(fo => fo.Name == fundOwner.Name))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Fund.DailyNavs),
                Code = ErrorCodesStore.DuplicateKey,
                Description = "The fund owner name is already in use."
            });

        _errorManager.ThrowOnErrors();

        var newFundOwner = _mapper.Map<FundOwner>(fundOwner);

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("FundOwners", null, newFundOwner);
            await _db.FundOwners.AddAsync(newFundOwner);
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

        return Ok(_mapper.Map<FundOwnerListViewModel>(newFundOwner));
    }

    [HttpGet]
    [Route("owner/{fundOwnerId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader,Trader")]
#endif
    public async Task<ActionResult<FundOwnerDetailViewModel>> GetFundOwner(Guid fundOwnerId, CancellationToken ct)
    {
        var fundOwner = await _db.FundOwners
            .AsNoTracking()
            .Where(f => f.Id == fundOwnerId)
            .Include(f => f.Funds)
            .SingleOrDefaultAsync(ct);

        if (fundOwner == null)
            return NotFound();

        return Ok(_mapper.Map<FundOwnerDetailViewModel>(fundOwner));
    }

    [HttpPut]
    [Route("owner/{fundOwnerId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutFundOwner(Guid fundOwnerId, [FromBody] FundOwnerEditViewModel fundOwner)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedFundOwner = await _db.FundOwners
            .Where(f => f.Id == fundOwnerId)
            .SingleOrDefaultAsync();

        if (storedFundOwner == null)
            return NotFound();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("FundOwners", storedFundOwner, fundOwner);
            _mapper.Map(fundOwner, storedFundOwner);
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
    [Route("owner/{fundOwnerId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteFundOwner(Guid fundOwnerId)
    {
        var storedFundOwner = await _db.FundOwners
            .Where(c => c.Id == fundOwnerId)
            .SingleOrDefaultAsync();

        if (storedFundOwner == null)
            return NotFound();

        // checks for existing funds and return nice error messages.
        if (await _db.Funds.AnyAsync(f => f.FundOwnerId == fundOwnerId))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(FundOwner.Id),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "The fund owner has funds registered so can not be deleted."
            });

        _errorManager.ThrowOnErrors();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("FundOwners", storedFundOwner, null);
            _db.FundOwners.Remove(storedFundOwner);
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

    #region FundLayers

    /// <summary>
    /// Updates the LayerIndex for all items in the list. Because the LayerIndex is part
    /// of the primary key, updating is not possible. We have to remove and insert the
    /// updated index. To do so, the list must be ordered in the right direction so no
    /// duplicate key conflicts appear. For that same reason async doesn't work either.
    /// This action is a bad performing solution but because only a few layers will exist 
    /// this is not an issue.
    /// </summary>
    /// <param name="items">The items to </param>
    /// <param name="offset"></param>
    private void UpdateLayerIndex(List<FundLayer> items, int offset)
    {
        items.Sort((fl1, fl2) => (fl1.LayerIndex * -offset).CompareTo(fl2.LayerIndex * -offset));

        items.ForEach(fl =>
        {
            // First remove
            _db.FundLayers.Remove(fl);
            _db.SaveChanges();

            // Then add with the updated index.
            fl.LayerIndex = (byte)(fl.LayerIndex + offset);
            _db.FundLayers.Add(fl);
            _db.SaveChanges();
        });
    }

    [HttpGet]
    [Route("{fundId}/layers")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<IEnumerable<FundLayerViewModel>>> GetFundLayers(Guid fundId, CancellationToken ct)
    {
        var fundLayers = await _db.FundLayers
            .AsNoTracking()
            .Where(fl => fl.FundId == fundId)
            .OrderBy(fl => fl.LayerIndex)
            .Select(fl => _mapper.Map<FundLayerViewModel>(fl))
            .ToListAsync(ct);

        // TODO: Get the current holdings percentages based on the layer strategy

        return Ok(fundLayers);
    }

    [HttpGet]
    [Route("{fundId}/layers/{layerIndex}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<FundLayerViewModel>> GetFundLayers(Guid fundId, byte layerIndex, CancellationToken ct)
    {
        var fundLayer = await _db.FundLayers
            .AsNoTracking()
            .Where(fl => fl.FundId == fundId && fl.LayerIndex == layerIndex)
            .Select(fl => _mapper.Map<FundLayerViewModel>(fl))
            .FirstOrDefaultAsync(ct);

        if (fundLayer == null)
            return NotFound();

        return Ok(fundLayer);
    }

    [HttpPost]
    [Route("{fundId}/layers")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<FundLayerViewModel>> PostFundLayer(Guid fundId, [FromBody] FundLayerEditViewModel fundLayer)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        if (!await _db.Funds.AnyAsync(f => f.Id == fundId))
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(FundLayer.Fund),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced fund is not found."
            });

        _errorManager.ThrowOnErrors();

        var newFundLayer = _mapper.Map<FundLayer>(fundLayer);

        newFundLayer.FundId = fundId;
        var maxLayerIndex = await _db.FundLayers
            .Where(l => l.FundId == fundId)
            .MaxAsync(l => (byte?)l.LayerIndex) ?? 0;
        newFundLayer.LayerIndex = (byte)(maxLayerIndex + 1);

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("FundLayers", null, newFundLayer);
            await _db.FundLayers.AddAsync(newFundLayer);
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

        return Ok(_mapper.Map<FundLayerViewModel>(newFundLayer));
    }

    [HttpPut]
    [Route("{fundId}/layers/{layerIndex}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutFundLayer(Guid fundId, long layerIndex, [FromBody] FundLayerEditViewModel fundLayer)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedFundLayer = await _db.FundLayers
            .Where(fl => fl.FundId == fundId && fl.LayerIndex == layerIndex)
            .SingleOrDefaultAsync();

        if (storedFundLayer == null)
            return NotFound();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("FundLayers", storedFundLayer, fundLayer);
            _mapper.Map(fundLayer, storedFundLayer);
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
    [Route("{fundId}/layers/{layerIndex}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteFundLayer(Guid fundId, byte layerIndex)
    {
        if (await _db.Holdings.AnyAsync(h => h.FundId == fundId && h.LayerIndex == layerIndex))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(FundLayer.LayerIndex),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "There are holdings linked to the fund layer. The layer can not be removed."
            });

        _errorManager.ThrowOnErrors();

        var storedFundLayer = await _db.FundLayers
            .Where(fl => fl.FundId == fundId && fl.LayerIndex == layerIndex)
            .SingleOrDefaultAsync();

        if (storedFundLayer == null)
            return NotFound();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("FundLayers", storedFundLayer, null);

            _db.FundLayers.Remove(storedFundLayer);
            _db.SaveChanges();

            // Update LayerIndex above removed index
            var reIndex = await _db.FundLayers
                .Where(fl => fl.FundId == storedFundLayer.FundId && fl.LayerIndex > storedFundLayer.LayerIndex)
                .ToListAsync();

            UpdateLayerIndex(reIndex, -1);

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

    // TODO: Call for moving layerindex
    [HttpPatch]
    [Route("{fundId}/layers/{layerIndex}/move")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> MoveFundLayer(Guid fundId, byte layerIndex, [FromBody] IndexMoveViewModel fundLayerMove)
    {
        if (await _db.Holdings.AnyAsync(h => h.FundId == fundId
                && h.LayerIndex >= Math.Min(fundLayerMove.FromIndex, fundLayerMove.ToIndex)
                && h.LayerIndex <= Math.Max(fundLayerMove.FromIndex, fundLayerMove.ToIndex)))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(FundLayer.LayerIndex),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "There are holdings linked to the fund layers that are affected. The layer can not be moved."
            });

        _errorManager.ThrowOnErrors();

        var storedFundLayer = await _db.FundLayers
            .Where(fl => fl.FundId == fundId && fl.LayerIndex == layerIndex)
            .SingleOrDefaultAsync();

        if (storedFundLayer == null)
            return NotFound();

        // Get max index and do the moving.
        var maxLayerIndex = await _db.FundLayers
                        .Where(l => l.FundId == fundId)
                        .Select(l => l.LayerIndex)
                        .MaxAsync();
        fundLayerMove.FromIndex = layerIndex;
        fundLayerMove.ToIndex = Math.Max(1, Math.Min(fundLayerMove.ToIndex, maxLayerIndex));

        if (fundLayerMove.FromIndex == fundLayerMove.ToIndex)
            return Ok();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("FundLayers", storedFundLayer, fundLayerMove);

            _db.FundLayers.Remove(storedFundLayer);
            _db.SaveChanges();

            // Update LayerIndex between the from and to indexes
            if (fundLayerMove.FromIndex > fundLayerMove.ToIndex)
            {
                var reIndex = await _db.FundLayers
                    .Where(fl => fl.FundId == fundId
                        && fl.LayerIndex >= fundLayerMove.ToIndex
                        && fl.LayerIndex < fundLayerMove.FromIndex)
                    .ToListAsync();
                UpdateLayerIndex(reIndex, 1);
            }
            else
            {
                var reIndex = await _db.FundLayers
                    .Where(fl => fl.FundId == fundId
                        && fl.LayerIndex > fundLayerMove.FromIndex
                        && fl.LayerIndex <= fundLayerMove.ToIndex)
                    .ToListAsync();
                UpdateLayerIndex(reIndex, -1);
            }

            // And add it again with the new index
            storedFundLayer.LayerIndex = (byte)fundLayerMove.ToIndex;
            await _db.FundLayers.AddAsync(storedFundLayer);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }

        return Ok();
    }

    #endregion

    #region Fund categories

    [HttpGet]
    [Route("{fundId}/categories")]
    public async Task<ActionResult<IEnumerable<FundCategoryViewModel>>> GetFundCategories(Guid fundId, CancellationToken ct)
    {
        var fundCategories = await _fundService.GetFundCategories(fundId, ct);

        // TODO: Get current fund percentages for the categories

        return Ok(fundCategories
            .Select(fc => _mapper.Map<FundCategoryViewModel>(fc))
            .ToList());
    }

    [HttpGet]
    [Route("{fundId}/categories/{categoryId}")]
    public async Task<ActionResult<FundCategoryViewModel>> GetFundCategory(Guid fundId, Guid categoryId, CancellationToken ct)
    {
        var fundCategory = await _fundService.GetFundCategory(fundId, categoryId, ct);

        if (fundCategory == null)
            return NotFound();

        // TODO: Build mapper for FundCategory > FundCategoryModelView

        return Ok(_mapper.Map<FundCategoryViewModel>(fundCategory));
    }

    [HttpPost]
    [Route("{fundId}/categories")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader,Trader")]
#endif
    public async Task<ActionResult<FundCategoryViewModel>> PostFundCategory(Guid fundId, [FromBody] FundCategoryEditViewModel fundCategory)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var newFundCategory = _mapper.Map<FundCategory>(fundCategory);
        newFundCategory.FundId = fundId;

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("FundCategories", null, newFundCategory);
            await _db.FundCategories.AddAsync(newFundCategory);
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

        return Ok(_mapper.Map<FundCategoryViewModel>(newFundCategory));
    }

    [HttpPut]
    [Route("{fundId}/categories/{categoryId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader,Trader")]
#endif
    public async Task<IActionResult> PutFundCategory(Guid fundId, Guid categoryId, [FromBody] FundCategoryEditViewModel fundCategory)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedFundCategory = await _db.FundCategories
            .Where(fc => fc.FundId == fundId && fc.CategoryId == categoryId)
            .SingleOrDefaultAsync();

        if (storedFundCategory == null)
            return NotFound();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("FundCategories", storedFundCategory, fundCategory);
            _mapper.Map(fundCategory, storedFundCategory);
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
    [Route("{fundId}/categories/{categoryId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader,Trader")]
#endif
    public async Task<IActionResult> DeleteFundCategory(Guid fundId, Guid categoryId)
    {
        var storedFundCategory = await _db.FundCategories
            .Where(fc => fc.FundId == fundId && fc.CategoryId == categoryId)
            .SingleOrDefaultAsync();

        if (storedFundCategory == null)
            return NotFound();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("FundCategories", storedFundCategory, null);
            _db.FundCategories.Remove(storedFundCategory);
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

    #region Fund Holdings

    /// <summary>
    /// Gets the holdings for the current (latest) booking period for the selected fund.
    /// </summary>
    /// <param name="fundId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{fundId}/holdings")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader,Trader")]
#endif
    public async Task<ActionResult<IEnumerable<HoldingListViewModel>>> GetCurrentHoldings(
        Guid fundId, bool filterUnused = true, CancellationToken ct = default)
    {
        var holdings = await _fundService.GetCurrentFundHoldings(fundId, filterUnused, true, ct);

        if (holdings == null)
            return NotFound();

        return Ok(holdings.Select(h => _mapper.Map<HoldingListViewModel>(h)).ToArray());
    }

    /// <summary>
    /// Gets the holdings for the in a range of booking periods for the selected fund.
    /// </summary>
    /// <param name="fundId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{fundId}/holdings/history")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader,Trader")]
#endif
    public async Task<ActionResult<IEnumerable<HoldingListViewModel>>> GetHoldingsHistory(Guid fundId, string fromBookingPeriod, string toBookingPeriod, bool filterUnused = true)
    {
        // TODO: Filter out real values for non leadtraders
        var holdings = await _fundService.GetFundHoldings(fundId, fromBookingPeriod, toBookingPeriod, filterUnused);

        if (holdings == null)
            return NotFound();

        return Ok(holdings.Select(h => _mapper.Map<HoldingListViewModel>(h)).ToArray());
    }

    [HttpGet]
    [Route("{fundId}/holdings/{holdingId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<HoldingDetailViewModel>> GetHolding(Guid fundId, Guid holdingId)
    {
        var holding = await _db.Holdings
            .AsNoTracking()
            .Where(h => h.FundId == fundId && h.Id == holdingId)
            .Include(h => h.Currency)
            .Include(h => h.CryptoCurrency)
            .Include(h => h.Transfers)
            .AsSplitQuery()
            .SingleOrDefaultAsync();

        if (holding == null)
            return NotFound();

        return Ok(_mapper.Map<HoldingDetailViewModel>(holding));
    }

    /// <summary>
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="holding"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{fundId}/holdings")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<HoldingListViewModel>> PostHolding(Guid fundId, [FromBody] HoldingAddViewModel holding)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        // Check reference to currency, cryptocurrency or fund exists
        if (holding.CryptoId == null && string.IsNullOrEmpty(holding.CurrencyISOCode) && holding.SharesFundId == null)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = $"{nameof(Holding.Currency)} / {nameof(Holding.CryptoCurrency)}",
                Code = ErrorCodesStore.DoubleLinked,
                Description = "A holding must be linked to a currency, either a real world currency or a crypto currency, or an investment fund."
            });
        // Check if only one reference is given
        if ((holding.CryptoId != null && !string.IsNullOrEmpty(holding.CurrencyISOCode)) ||
            (holding.CryptoId != null && holding.SharesFundId != null) ||
            (!string.IsNullOrEmpty(holding.CurrencyISOCode) && holding.SharesFundId != null))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = $"{nameof(Holding.Currency)} + {nameof(Holding.CryptoCurrency)}",
                Code = ErrorCodesStore.DoubleLinked,
                Description = "The holding can only be linked to one currency, either a real world currency or a crypto currency, or an investment fund."
            });

        var currentBookingPeriod = await _fundService.CurrentBookingPeriod(fundId);

        // Here we check to see if the new holding being created is for a cryptocurrency AND if so, whether a holding
        // for this specific cryptocurrency already exists.
        if (holding.CryptoId != null &&
            await _db.Holdings.AnyAsync(h => h.FundId == fundId && h.CryptoId == holding.CryptoId && h.BookingPeriod == currentBookingPeriod))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Holding.CryptoCurrency),
                Code = ErrorCodesStore.DuplicateKey,
                Description = "The holding for the crypto currency already exists."
            });
        if (!string.IsNullOrEmpty(holding.CurrencyISOCode) &&
            await _db.Holdings.AnyAsync(h => h.FundId == fundId && h.CurrencyISOCode == holding.CurrencyISOCode && h.BookingPeriod == currentBookingPeriod))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Holding.Currency),
                Code = ErrorCodesStore.DuplicateKey,
                Description = "The holding for the currency already exists."
            });
        if (holding.SharesFundId != null &&
            await _db.Holdings.AnyAsync(h => h.FundId == fundId && h.SharesFundId == holding.SharesFundId && h.BookingPeriod == currentBookingPeriod))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Holding.SharesFund),
                Code = ErrorCodesStore.DuplicateKey,
                Description = "The holding for the shares already exists."
            });

        // Preventing non-active crypto's from being added as new holdings
        if (holding.CryptoId != null &&
            !await _db.CryptoCurrencies.AnyAsync(c => c.Id == holding.CryptoId && c.Active))
        {
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Holding.CryptoCurrency),
                Code = ErrorCodesStore.InvalidValue,
                Description = "Non-active crypto currency holdings cannot be added."
            });
        }

        // Preventing non-active fund investment
        if (holding.SharesFundId != null &&
            await _db.Funds.AnyAsync(f => f.Id == holding.SharesFundId && f.DateEnd != null && f.DateEnd < DateTime.Today && f.DateEnd > DateTime.MinValue))
        {
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Holding.SharesFund),
                Code = ErrorCodesStore.InvalidValue,
                Description = "Ended fund holdings cannot be added."
            });
        }
        // Prevent circular references in fund investments
        if (holding.SharesFundId != null &&
            await _db.Holdings.AnyAsync(h => h.FundId == holding.SharesFundId && h.SharesFundId == fundId))
        {
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Holding.SharesFund),
                Code = ErrorCodesStore.InvalidValue,
                Description = "The referenced fund has an investment in the current fund. This circular reference can not be made."
            });
        }

        _errorManager.ThrowOnErrors();

        var newHolding = _mapper.Map<Holding>(holding);

        newHolding.FundId = fundId;
        newHolding.BookingPeriod = await _fundService.CurrentBookingPeriod(fundId);
        var startBookinPeriod = _bookingPeriodHelper.GetPeriodStartDateTime(newHolding.BookingPeriod).UtcDateTime;
        var endBookingPeriod = _bookingPeriodHelper.GetPeriodEndDateTime(newHolding.BookingPeriod).UtcDateTime;
        if (newHolding.StartDateTime < startBookinPeriod || newHolding.StartDateTime > endBookingPeriod)
        {
            newHolding.StartDateTime = startBookinPeriod;
        }
        if (newHolding.EndBalance == 0)
        {
            newHolding.EndBalance = newHolding.StartBalance;
        }

        // Calculate the USD and BTC prices if one is missing
        var prices = await CalcPrices(
            newHolding.StartUSDPrice,
            newHolding.StartBTCPrice,
            newHolding.StartDateTime,
            newHolding.CurrencyISOCode,
            newHolding.CryptoId,
            newHolding.SharesFundId);
        newHolding.StartUSDPrice = prices.Item1;
        newHolding.StartBTCPrice = prices.Item2;
        newHolding.EndUSDPrice = prices.Item1;
        newHolding.EndBTCPrice = prices.Item2;

        var savedHolding = await _fundService.AddHolding(newHolding);

        return Ok(_mapper.Map<HoldingListViewModel>(savedHolding));
    }

    [HttpPut]
    [Route("{fundId}/holdings/{holdingId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutHolding(Guid fundId, Guid holdingId, [FromBody] HoldingEditViewModel holding)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedHolding = await _db.Holdings
            .Where(h => h.FundId == fundId && h.Id == holdingId)
            .SingleOrDefaultAsync();

        if (storedHolding == null)
            return NotFound();

        // Take over default values from the stored holding
        holding.StartDateTime = holding.StartDateTime == DateTime.MinValue
            ? storedHolding.StartDateTime
            : holding.StartDateTime;
        holding.EndDateTime = holding.EndDateTime == null
            ? storedHolding.EndDateTime
            : holding.EndDateTime;
        if (holding.StartUSDPrice == 0 && holding.StartBTCPrice == 0)
        {
            holding.StartUSDPrice = storedHolding.StartUSDPrice;
            holding.StartBTCPrice = storedHolding.StartBTCPrice;
        }
        if (holding.EndUSDPrice == 0 && holding.EndBTCPrice == 0)
        {
            holding.EndUSDPrice = storedHolding.EndUSDPrice;
            holding.EndBTCPrice = storedHolding.EndBTCPrice;
        }

        // Calculate the USD and BTC prices if one is missing
        var prices = await CalcPrices(
            holding.StartUSDPrice,
            holding.StartBTCPrice,
            holding.StartDateTime,
            storedHolding.CurrencyISOCode,
            storedHolding.CryptoId,
            storedHolding.SharesFundId);
        holding.StartUSDPrice = prices.Item1;
        holding.StartBTCPrice = prices.Item2;

        prices = await CalcPrices(
            holding.EndUSDPrice,
            holding.EndBTCPrice,
            holding.EndDateTime ?? _bookingPeriodHelper.GetPeriodEndDateTime(storedHolding.BookingPeriod),
            storedHolding.CurrencyISOCode,
            storedHolding.CryptoId,
            storedHolding.SharesFundId);
        holding.EndUSDPrice = prices.Item1;
        holding.EndBTCPrice = prices.Item2;

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("Holdings", storedHolding, holding);

            // Prepare automated changes in end balance when starbalance changes
            var storedStartBalance = storedHolding.StartBalance;
            bool recalcEndBalance = storedHolding.StartBalance != holding.StartBalance &&
                (storedHolding.EndBalance == holding.EndBalance || holding.EndBalance == 0);

            // Map the updated values to the stored holding
            _mapper.Map(holding, storedHolding);

            // And finally recalc end balance if needed
            if (recalcEndBalance)
            {
                storedHolding.EndBalance += (holding.StartBalance - storedStartBalance);
            }

            await _db.SaveChangesAsync();

            if (storedStartBalance != holding.StartBalance)
            {
                // Recalc start and end percentages
                _ = await _fundService.RecalcPercentages(storedHolding.FundId, storedHolding.BookingPeriod);
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

        return Ok();
    }

    [HttpDelete]
    [Route("{fundId}/holdings/{holdingId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteHolding(Guid fundId, Guid holdingId)
    {
        var storedHolding = await _db.Holdings
            .Where(h => h.FundId == fundId && h.Id == holdingId)
            .SingleOrDefaultAsync();

        if (storedHolding == null)
            return NotFound();

        // Tests, referenced by Holdings, transfers or when period is closed, no delete!
        if (storedHolding.StartBalance != 0 || storedHolding.NavBalance != 0 || storedHolding.EndBalance != 0)
            _errorManager.AddValidationError(HttpStatusCode.NotAcceptable, new ErrorInformationItem
            {
                Field = nameof(Holding.StartBalance) + "/" + nameof(Holding.NavBalance) + "/" + nameof(Holding.EndBalance),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "The holding has either a start balance or an end balance that is not 0. Make sure the balances are 0 before removing the holding."
            });

        if (storedHolding.PeriodClosedDateTime != null)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Holding.Id),
                Code = ErrorCodesStore.ItemIsLocked,
                Description = "The holding is a closed period. It can not be removed."
            });

        if (await _db.Holdings.AnyAsync(h => h.PreviousHoldingId == storedHolding.Id))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Holding.PreviousHolding),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "The holding is referenced by a follow up holding. It can not be removed."
            });

        if (await _db.Transfers.AnyAsync(t => t.HoldingId == storedHolding.Id))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Transfer.Holding),
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "The holding is referenced by a transfer. It can not be removed."
            });

        _errorManager.ThrowOnErrors();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("Holdings", storedHolding, null);
            _db.Holdings.Remove(storedHolding);
            await _db.SaveChangesAsync();

            // Recalc start and end percentages
            _ = await _fundService.RecalcPercentages(storedHolding.FundId, storedHolding.BookingPeriod);

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
    /// CaclPrices calculates the current prices for the holding. It is different 
    /// than the NAV calculations because here the the current rates and listings 
    /// (latest) are used for the calculation, and when one of the prices is 
    /// available, that one will be used. This is used for new holdings and 
    /// manually updated values.
    /// </summary>
    /// <param name="usdPrice"></param>
    /// <param name="btcPrice"></param>
    /// <param name="date"></param>
    /// <param name="currencyIsoCode"></param>
    /// <param name="cryptoId"></param>
    /// <param name="fundId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<(decimal, decimal)> CalcPrices(decimal usdPrice, decimal btcPrice, DateTimeOffset date, string currencyIsoCode, Guid? cryptoId, Guid? fundId, CancellationToken cancellationToken = default)
    {
        // Calc BTC price if USD price is given only
        if (btcPrice == 0 && usdPrice == 0)
        {
            // Try to find listing or currency rate
            if (!string.IsNullOrEmpty(currencyIsoCode))
            {
                var rate = await _currencyService.GetCurrencyRatingByDate(currencyIsoCode, date, cancellationToken);
                usdPrice = 1 / rate?.USDRate ?? 0;
            }
            if (cryptoId != null)
            {
                var listing = await _cryptoService.GetListingByDate((Guid)cryptoId, date, _cryptoService.PreferedListingSource, false, cancellationToken);
                usdPrice = listing?.USDPrice ?? 0;
                btcPrice = listing?.BTCPrice ?? 0;
            }
            if (fundId != null)
            {
                // Get the share USD value, which is the NAV value divided by the USDRate of the reportingcurrency
                var nav = await _fundService.GetNavByDate((Guid)fundId, date, cancellationToken);
                usdPrice = nav?.ShareNAV / nav?.CurrencyRate.USDRate ?? 1;
            }
        }
        if (btcPrice == 0 && usdPrice != 0)
        {
            var listing = await _cryptoService.GetListingByDate(_cryptoService.BtcGuid, date, _cryptoService.PreferedListingSource, false, cancellationToken);
            if (listing != null)
            {
                btcPrice = usdPrice / listing.USDPrice;
            }
        }
        if (btcPrice != 0 && usdPrice == 0)
        {
            var listing = await _cryptoService.GetListingByDate(_cryptoService.BtcGuid, date, _cryptoService.PreferedListingSource, false, cancellationToken);
            if (listing != null)
            {
                usdPrice = btcPrice * listing.USDPrice;
            }
        }

        return (usdPrice, btcPrice);
    }

    #endregion

    #region FundHoldings.LayerIndex

    /// <summary>
    /// This PUT req is meant to assign the appropriate fund LayerIndex to a holding whose LayerIndex is 0 - meaning, not yet set.
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="holdingId"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{fundId}/holdings/{holdingId}/assign_layer_idx")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader,Trader")]
#endif
    public async Task<IActionResult> AssignLayerIdx(Guid fundId, Guid holdingId)
    {
        var storedHolding = await _db.Holdings
            .Where(h => h.FundId == fundId && h.Id == holdingId)
            .SingleOrDefaultAsync();

        if (storedHolding == null)
            return NotFound();

        storedHolding.LayerIndex = await _layerIdxService.IdxAssignmentStrategy(storedHolding);
        await _db.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// This PUT req is meant to assign the appropriate fund LayerIndex to ALL holdings in a given fund(Id).
    /// Only those holdings which are crypto holdings & those with LayerIndex=0 are taken into consideration
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="holdingId"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{fundId}/holdings/assign_layer_indeces")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> AssignFundHoldingsLayerIndeces(Guid fundId)
    {
        var bookingPeriod = await _fundService.CurrentBookingPeriod(fundId);
        var holdings = await _db.Holdings
            .Where(h => h.FundId == fundId && h.BookingPeriod == bookingPeriod && h.LayerIndex == 0)
            .Include(h => h.Currency)
            .Include(h => h.CryptoCurrency)
            .OrderBy(h => h.Currency.Name ?? h.CryptoCurrency.Name)
            .AsSingleQuery()
            .ToListAsync();

        if (holdings == null) return NotFound();

        foreach (var holding in holdings)
        {
            holding.LayerIndex = await _layerIdxService.IdxAssignmentStrategy(holding);
        }
        await _db.SaveChangesAsync();

        return Ok();
    }

    #endregion

    #region DailyNavs

    [HttpGet]
    [Route("{fundId}/dailynavs")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<IEnumerable<DailyNavViewModel>>> GetDailyNavs(Guid fundId, string from, string to)
    {
        var dateFrom = from.ParamToUtcDate(DateTime.Today.AddMonths(-1));
        var dateTo = to.ParamToUtcDate(DateTime.MaxValue);

        var navs = await _db.Navs
            .AsNoTracking()
            .Where(nav => nav.FundId == fundId && nav.DateTime >= dateFrom && nav.DateTime <= dateTo)
            .Include(nav => nav.CurrencyRate)
            .OrderByDescending(dn => dn.DateTime)
            .AsSingleQuery()
            .Select(nav => _mapper.Map<DailyNavViewModel>(nav))
            .ToListAsync();

        return Ok(navs);
    }

    [HttpPost]
    [Route("{fundId}/dailynavs")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<DailyNavViewModel>> PostDailyNav(Guid fundId, [FromBody] DailyNavEditViewModel dailyNav)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        if (!await _db.Funds.AnyAsync(f => f.Id == fundId))
            _errorManager.AddValidationError(HttpStatusCode.NotFound, new ErrorInformationItem
            {
                Field = nameof(Nav.Fund),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "Referenced fund is not found."
            });

        _errorManager.ThrowOnErrors();

        var newDailyNav = _mapper.Map<Nav>(dailyNav);

        newDailyNav.FundId = fundId;

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("DailyNavs", null, newDailyNav);
            await _db.Navs.AddAsync(newDailyNav);
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

        return Ok(_mapper.Map<DailyNavViewModel>(newDailyNav));
    }

    [HttpPut]
    [Route("{fundId}/dailynavs/{dailyNavId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutFundLayer(Guid fundId, Guid dailyNavId, [FromBody] DailyNavEditViewModel dailyNav)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var storedDailyNav = await _db.Navs
            .Where(dn => dn.FundId == fundId && dn.Id == dailyNavId)
            .SingleOrDefaultAsync();

        if (storedDailyNav == null)
            return NotFound();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("DailyNavs", storedDailyNav, dailyNav);
            _mapper.Map(dailyNav, storedDailyNav);
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
    [Route("{fundId}/dailynavs/{dailyNavId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteDailyNav(Guid fundId, Guid dailyNavId)
    {
        var storedDailyNav = await _db.Navs
            .Where(dn => dn.FundId == fundId && dn.Id == dailyNavId)
            .SingleOrDefaultAsync();

        if (storedDailyNav == null)
            return NotFound();

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await _changeLogService.AddChangeLogAsync("DailyNavs", storedDailyNav, null);
            _db.Navs.Remove(storedDailyNav);
            _db.SaveChanges();

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

    #region BookingPeriods

    [HttpPost]
    [Route("{fundId}/closehistory")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<string>> CloseAllBookingPeriods(Guid fundId)
    {
        return Ok(await _fundService.CloseAllBookingPeriods(fundId));
    }

    [HttpPost]
    [Route("{fundId}/closeperiod")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<string>> CloseBookingPeriod(Guid fundId, [FromBody] FundClosePeriodViewModel closePeriod)
    {
        return Ok(await _fundService.CloseBookingPeriod(fundId, closePeriod.Period, closePeriod.Recalculate));
    }

    [HttpPost]
    [Route("{fundId}/rollback_closeperiod")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<string>> RollbackCloseBookingPeriod(Guid fundId)
    {
        return Ok(await _fundService.RollbackCloseBookingPeriod(fundId));
    }
    #endregion

    #region DailyNavs Calculations

    [HttpPost]
    [Route("{fundId}/calcdailynav/{date}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<DailyNavViewModel>> CalcDailyNav(Guid fundId, DateTime date)
    {
        var nav = await _fundService.CreateDailyNAV(fundId, date);

        return Ok(_mapper.Map<DailyNavViewModel>(nav));
    }

    [HttpPost]
    [Route("{fundId}/calcperiodnavs")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<string>> CalcDailyNavsForPeriod(Guid fundId, [FromBody] FundClosePeriodViewModel closePeriod)
    {
        return Ok(await _fundService.CalcDailyNavsForPeriod(fundId, closePeriod.Period, closePeriod.Recalculate));
    }
    #endregion
}
