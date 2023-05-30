using Hodl.Api.ViewModels.StatisticsModels;

namespace Hodl.Api.Controllers.Statistics;

[ApiController]
[Route("statistics")]
public class StatisticsController : BaseController
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(
        IStatisticsService statisticsService,
        IMapper mapper,
        ILogger<StatisticsController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _statisticsService = statisticsService;
    }


    [HttpGet]
    [Route("averageprices/{baseAssetId}/{orderDirection}")]
    public async Task<ActionResult<AssetPriceStats>> AveragePrices(Guid baseAssetId, OrderDirection orderDirection = OrderDirection.Buy)
    {
        if (orderDirection == OrderDirection.Unknown)
            return BadRequest("Orderdirection must be Buy or Sell.");

        var averagePrices = await _statisticsService.GetAveragePrices(baseAssetId, orderDirection);

        return Ok(averagePrices);
    }
}
