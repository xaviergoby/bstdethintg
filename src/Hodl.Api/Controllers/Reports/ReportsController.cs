using Hodl.Api.ViewModels.ReportModels;

namespace Hodl.Api.Controllers.Reports;

[ApiController]
[Route("reports")]
public class ReportsController : BaseController
{
    private readonly HodlDbContext _db;
    private readonly IReportService _reportService;

    public ReportsController(
        HodlDbContext dbContext,
        IReportService reportService,
        IMapper mapper,
        ILogger<ReportsController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _db = dbContext;
        _reportService = reportService;
    }

    /// <summary>
    /// Gets a list of funds to select a report from.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("funds")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<ReportFundSelectionViewModel[]>> GetFunds(CancellationToken ct)
    {
        var funds = await _db.Funds
            .AsNoTracking()
            .Include(f => f.FundOwner)
            .OrderBy(f => f.FundName)
            .AsSingleQuery()
            .Select(f => _mapper.Map<ReportFundSelectionViewModel>(f))
            .ToArrayAsync(ct);

        return Ok(funds);
    }

    [HttpGet]
    [Route("{fundId}/bookingperiods")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<string[]>> GetBookingperiods(Guid fundId, CancellationToken ct)
    {
        var bookingperiods = await _db.Navs
            .AsNoTracking()
            .Where(n => n.FundId == fundId && n.Type == NavType.Period)
            .Select(n => n.BookingPeriod)
            .Distinct()
            .OrderByDescending(n => n)
            .ToArrayAsync(ct);

        return Ok(bookingperiods);
    }

    [HttpGet]
    [Route("{fundId}/{bookingPeriod}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<ReportFundViewModel>> GetReport(Guid fundId, string bookingPeriod, CancellationToken ct)
    {
        return Ok(await _reportService.GetReportInternal(fundId, bookingPeriod, ct));
    }
}
