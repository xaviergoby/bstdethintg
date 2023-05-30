using Hodl.Api.ViewModels.CurrencyModels;
using Hodl.Api.ViewModels.SandboxModels;

namespace Hodl.Api.Controllers.Sandbox;

[ApiController]
[Route("sandbox")]
public class SandboxController : BaseController
{
    private readonly ISandboxService _sandboxService;
    private readonly IStatisticsService _statisticsService;
    private readonly IUserResolver _userResolver;
    private readonly IAppConfigService _appconfigService;

    public SandboxController(
        ISandboxService sandboxService,
        IStatisticsService statisticsService,
        IUserResolver userResolver,
        IAppConfigService appconfigService,
        IMapper mapper,
        ILogger<SandboxController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _sandboxService = sandboxService;
        _statisticsService = statisticsService;
        _userResolver = userResolver;
        _appconfigService = appconfigService;
    }



    [HttpGet]
    [Route("{fundId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader, Trader")]
#endif
    public async Task<ActionResult<IEnumerable<SandboxHoldingViewModel>>> GetSandboxData(Guid fundId, CancellationToken ct)
    {
        var holdings = await _sandboxService.GetSandboxHoldings(fundId, ct);
        var sandBoxHoldings = holdings.Select(h => _mapper.Map<SandboxHoldingViewModel>(h)).ToArray();

        // Now add the missing data
        foreach (var sandboxHolding in sandBoxHoldings)
        {
            // Calculate the average ENTRY prices
            var avgPrices = await _statisticsService.GetAveragePrices(sandboxHolding.CryptoCurrency.Id, OrderDirection.Buy, ct);
            var fundAssetPrice = avgPrices.AssetFundAggPriceStats.SingleOrDefault(s => s.FundId.Equals(fundId));

            // Only add data when there is actually payd for the tokens.
            if (fundAssetPrice != null && fundAssetPrice.Total > 0)
            {
                sandboxHolding.EntryAmount = fundAssetPrice.Amount;
                sandboxHolding.EntryTotal = fundAssetPrice.Total;
            }

            // Calculate the average EXIT prices
            avgPrices = await _statisticsService.GetAveragePrices(sandboxHolding.CryptoCurrency.Id, OrderDirection.Sell, ct);
            fundAssetPrice = avgPrices.AssetFundAggPriceStats.SingleOrDefault(s => s.FundId.Equals(fundId));

            // Only add data when there is actually payd for the tokens.
            if (fundAssetPrice != null && fundAssetPrice.Total > 0)
            {
                sandboxHolding.ExitAmount = fundAssetPrice.Amount;
                sandboxHolding.ExitTotal = fundAssetPrice.Total;
            }
        }

        return Ok(sandBoxHoldings);
    }

    [HttpGet]
    [Route("currencies")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader,Trader")]
#endif
    public async Task<ActionResult<IEnumerable<CryptoCurrencyListViewModel>>> GetSandboxCurrencies(CancellationToken ct)
    {
        var cryptos = await _sandboxService.GetSandboxCurrencies(ct);

        return Ok(cryptos.Select(c => _mapper.Map<CryptoCurrencyListViewModel>(c)));
    }

    [HttpPut]
    [Route("currencies")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<CryptoCurrencyListViewModel>> PutSandboxCurrencies(Guid[] guids)
    {
        var user = await _userResolver.GetUser();
        _ = await _appconfigService.SetAppConfigAsync(AppConfigs.SANDBOX_CURRENCIES, guids, user?.Roles.First(), default);

        return Ok();
    }


}
