using Hodl.Api.ViewModels.DashboardModels;
using Hodl.Api.ViewModels.FundModels;

namespace Hodl.Api.Controllers.Dashboard;

[ApiController]
[Route("dashboard")]
public class DashboardController : BaseController
{
    private readonly HodlDbContext _db;
    private readonly IBookingPeriodHelper _bookingPeriodHelper;
    private readonly IFundService _fundService;
    private readonly IReportService _reportService;

    public DashboardController(
        HodlDbContext dbContext,
        IBookingPeriodHelper bookingPeriodHelper,
        IFundService fundService,
        IReportService reportService,
        IMapper mapper,
        ILogger<DashboardController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _db = dbContext;
        _bookingPeriodHelper = bookingPeriodHelper;
        _fundService = fundService;
        _reportService = reportService;
    }

    private async Task<PeriodNavViewModel> GetCurrentNavViewModel(Guid fundId, CancellationToken ct)
    {
        return _mapper.Map<PeriodNavViewModel>(await _fundService.GetCurrentNav(fundId, ct));
    }

    /// <summary>
    /// Gets all the funds with basic card info for the dashboard overview.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<DashboardFundCardView[]>> Get(CancellationToken ct)
    {
        var currentBookingPeriod = _bookingPeriodHelper.CalcBookingPeriod(DateTimeOffset.UtcNow);
        var funds = await _fundService.GetFundCards(ct);

        var fundCards = funds.Select(f => _mapper.Map<DashboardFundCardView>(f)).ToArray();

        // And add current/latest NAV to the fund
        foreach (var fundCard in fundCards)
        {
            var fund = funds.Single(f => f.Id == fundCard.Id);
            // Select the holdings for the last active periods
            fund.Holdings = await _fundService.GetCurrentFundHoldings(fund.Id, true, true, ct);

            fundCard.Nav = await GetCurrentNavViewModel(fundCard.Id, ct);
            fundCard.CurrentBookingPeriod = await _fundService.CurrentBookingPeriod(fundCard.Id, ct);
            fundCard.CategoryGroups = await _fundService.GetFundGategoryGroups(fundCard.Id, ct);

            var holdings = await _fundService.CalcHoldingDistribution(fund.Holdings, ct);
            fundCard.Holdings = holdings.Select(h => _mapper.Map<DashboardHoldingCardView>(h)).ToArray();

            AddLayerNames(fundCard.Holdings, fundCard.Layers.Select(l => l as FundLayerViewModel).ToArray());

            var layerDistribution = await _fundService.CalcLayerDistribution(fund.Holdings, ct);
            foreach (var layer in fundCard.Layers)
            {
                if (layerDistribution.TryGetValue(layer.LayerIndex, out SumRecord rec))
                {
                    layer.CurrentPercentage = rec.TotalSharePercentage;
                    layer.NumberOfHoldings = rec.NumberOfItems;
                }
            }
        }

        return Ok(fundCards);
    }

    /// <summary>
    /// Get group distributions.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("{fundId}/categories/{categoryGroup}")]
    public async Task<ActionResult<DashboardFundCategoryCardView[]>> GetCategoryDistributions(
        Guid fundId, string categoryGroup, CancellationToken ct)
    {
        var currentBookingPeriod = _bookingPeriodHelper.CalcBookingPeriod(DateTimeOffset.UtcNow);

        var fund = await _db.Funds
            .AsNoTracking()
            .Where(f => f.Id == fundId)
            .Include(f => f.FundCategories.Where(fc => fc.Category.Group == categoryGroup))
            .ThenInclude(fc => fc.Category)
            .AsSplitQuery()
            .SingleOrDefaultAsync(ct);

        if (fund == null)
            return NotFound();

        // Add holdings and latest prices
        fund.Holdings = await _fundService.GetCurrentFundHoldings(fund.Id, true, true, ct);
        var holdings = await _fundService.CalcHoldingDistribution(fund.Holdings, ct);
        var categorySums = await _fundService.CalcCategoryDistribution(fund.FundCategories, holdings, ct);

        // Map to the output type and then reflect the summations of the holdings
        var fundCategories = fund.FundCategories.Select(f => _mapper.Map<DashboardFundCategoryCardView>(f)).ToArray();

        // And add current/latest NAV to the fund
        foreach (var fundCategory in fundCategories)
        {
            if (categorySums.TryGetValue(fundCategory.CategoryId, out SumRecord rec))
            {
                fundCategory.BTCValue = rec.BTCValue;
                fundCategory.USDValue = rec.USDValue;
                fundCategory.AllocationPercentage = rec.TotalSharePercentage;
                fundCategory.NumberOfItems = rec.NumberOfItems;
            }
        }

        return Ok(fundCategories);
    }

    /// <summary>
    /// Get Holdings dataset.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("{fundId}/holdings")]
    public async Task<ActionResult<DashboardHoldingTableView[]>> GetHoldingsData(Guid fundId, CancellationToken ct)
    {
        var rawHoldings = await _fundService.GetCurrentFundHoldings(fundId, true, true, ct);
        var holdings = await _fundService.CalcHoldingDistribution(rawHoldings, ct);

        var layers = await _db.FundLayers
            .AsNoTracking()
            .Where(l => l.FundId == fundId)
            .Select(l => _mapper.Map<FundLayerViewModel>(l))
            .ToArrayAsync(ct);

        // Map to the output type and then reflect the summations of the holdings
        var holdingsViews = holdings.Select(h => _mapper.Map<DashboardHoldingTableView>(h)).ToArray();
        AddLayerNames(holdingsViews, layers);

        return Ok(holdingsViews);
    }

    /// <summary>
    /// Get Trade summary dataset. Also includes the transfers transactions. 
    /// All the money movements are summed and displayed in four categories: 
    /// inflow/outflow, trades in/out, staking summary and fees.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("{fundId}/summary/trades")]
    public async Task<ActionResult<DashboardTradeSummaryTableView[]>> GetTradeSummary(Guid fundId, CancellationToken ct)
    {
        var bookingPeriod = await _fundService.CurrentBookingPeriod(fundId, ct);
        DateTimeOffset periodStart = _bookingPeriodHelper.GetPeriodStartDateTime(bookingPeriod);
        DateTimeOffset periodEnd = _bookingPeriodHelper.GetPeriodEndDateTime(bookingPeriod);

        var fund = await _db.Funds
            .AsNoTracking()
            .Where(f => f.Id == fundId)
            .Include(f => f.FundOwner)
            .Include(f => f.Layers.OrderBy(l => l.LayerIndex))
            .Include(f => f.Holdings.Where(h => h.BookingPeriod == bookingPeriod))
            .ThenInclude(h => h.Currency)
            .Include(f => f.Holdings.Where(h => h.BookingPeriod == bookingPeriod))
            .ThenInclude(h => h.CryptoCurrency)
            .AsSplitQuery()
            .SingleOrDefaultAsync(ct) ??
            throw new NotFoundException("Fund not found");

        // Get the trades and transfers
        var holdingIds = fund.Holdings.Select(h => h.Id).ToArray();
        var transfers = await _db.Transfers
            .Where(t => holdingIds.Contains(t.HoldingId))
            .Include(t => t.Holding)
            .ThenInclude(h => h.Currency)
            .Include(t => t.Holding)
            .ThenInclude(h => h.CryptoCurrency)
            .Include(t => t.FeeHolding)
            .ThenInclude(h => h.Currency)
            .Include(t => t.FeeHolding)
            .ThenInclude(h => h.CryptoCurrency)
            .OrderBy(t => t.DateTime)
            .ThenBy(t => t.Holding.CurrencyISOCode)
            .ThenBy(t => t.Holding.CryptoCurrency.Symbol)
            .AsSplitQuery()
            .ToArrayAsync(ct);

        var trades = await _db.Trades
            .Where(t => t.BookingPeriod == bookingPeriod ||
            (
                string.IsNullOrWhiteSpace(t.BookingPeriod) &&
                t.DateTime >= periodStart.UtcDateTime &&
                t.DateTime < periodEnd.UtcDateTime
            ))
            .Include(t => t.Order)
            .ThenInclude(o => o.OrderFundings.Where(f => f.FundId == fundId && f.OrderAmount != 0))
            .Include(t => t.Order)
            .ThenInclude(o => o.BaseAsset)
            .Include(t => t.Order)
            .ThenInclude(o => o.QuoteAsset)
            .Include(t => t.FeeCurrency)
            .OrderBy(t => t.DateTime)
            .AsSplitQuery()
            .ToArrayAsync(ct);

        var tradesums = _reportService.GetTradeSummary(fund, transfers, trades);

        return Ok(tradesums.Select(s => _mapper.Map<DashboardTradeSummaryTableView>(s)).ToArray());
    }

    /// <summary>
    /// Get staking overview. This is the holdings overview where staking is 
    /// set and selects the rewards with it.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("{fundId}/summary/staking")]
    public async Task<ActionResult<DashboardTradeSummaryTableView[]>> GetStakingSummary(Guid fundId, CancellationToken ct)
    {
        var rawHoldings = await _fundService.GetCurrentFundHoldings(fundId, false, false, ct);

        var stakeHoldings = rawHoldings
            .Where(h => h.CryptoCurrency != null && h.CryptoCurrency.IsLocked)
            .Select(h => _mapper.Map<DashboardTradeSummaryTableView>(h)).ToArray();

        // TODO: Select the linked free holdings for the calculation of staking rewards (current rewards do not match locked currencies)

        var holdingIds = rawHoldings.Select(h => h.Id).ToArray();
        var transfers = await _db.Transfers
            .Where(t => holdingIds.Contains(t.HoldingId) && t.TransactionType == TransactionType.Reward)
            .Include(t => t.Holding)
            .ThenInclude(h => h.CryptoCurrency)
            .OrderBy(t => t.DateTime)
            .ThenBy(t => t.Holding.CryptoCurrency.Symbol)
            .ToArrayAsync(ct);

        // Now walk the transfers and add the numbers
        foreach (var transfer in transfers)
        {
            var holding = stakeHoldings.SingleOrDefault(h => h.CurrencySymbol.StartsWith(transfer.Holding.CryptoCurrency.Symbol));

            if (holding == null) continue;

            holding.StakingRewards += transfer.TransferAmount;
        }

        return Ok(stakeHoldings);
    }

    /// <summary>
    /// Get the transfers overview for the fund to be able to see in which 
    /// wallets of accounts the funding should be. 
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("{fundId}/transfers")]
    public async Task<ActionResult<DashboardTransfersLogModel[]>> GetTransfers(Guid fundId, CancellationToken ct)
    {
        // Select the holdings for the last active periods
        var holdings = await _fundService.GetCurrentFundHoldings(fundId, false, false, ct);

        var holdingIds = holdings.Select(h => h.Id).ToArray();

        var transfers = await _db.Transfers
            .Include(t => t.Holding)
            .ThenInclude(h => h.Currency)
            .Include(t => t.Holding)
            .ThenInclude(h => h.CryptoCurrency)
            .Include(t => t.FeeHolding)
            .ThenInclude(h => h.Currency)
            .Include(t => t.FeeHolding)
            .ThenInclude(h => h.CryptoCurrency)
            .Where(t => holdingIds.Contains(t.HoldingId))
            .OrderBy(t => t.DateTime)
            .ThenBy(t => t.Holding.CurrencyISOCode)
            .ThenBy(t => t.Holding.CryptoCurrency.Symbol)
            .Select(t => _mapper.Map<DashboardTransfersLogModel>(t))
            .AsSplitQuery()
            .ToArrayAsync(ct);

        return Ok(transfers);
    }

    private static void AddLayerNames(ICollection<DashboardHoldingCardView> holdings, ICollection<FundLayerViewModel> layers)
    {
        foreach (var holding in holdings)
        {
            holding.LayerName = layers.FirstOrDefault(l => l.LayerIndex == holding.LayerIndex)?.Name;
        }
    }
}
