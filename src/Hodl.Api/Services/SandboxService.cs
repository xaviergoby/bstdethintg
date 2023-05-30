namespace Hodl.Api.Services;

public class SandboxService : ISandboxService
{
    private readonly HodlDbContext _db;
    private readonly IAppConfigService _appConfigService;
    private readonly IFundService _fundService;

    public SandboxService(
        HodlDbContext dbContext,
        IAppConfigService appConfigService,
        IFundService fundService
        )
    {
        _db = dbContext;
        _appConfigService = appConfigService;
        _fundService = fundService;
    }

    public async Task<IEnumerable<Holding>> GetSandboxHoldings(Guid fundId, CancellationToken cancellationToken = default)
    {
        var guids = await GetSandboxCurrenciesGuids(cancellationToken);
        var holdings = await _fundService.CalcHoldingDistribution(
            await _fundService.GetCurrentFundHoldings(fundId, cancellationToken: cancellationToken), cancellationToken);

        var selection = holdings
            .Where(h => h.CryptoId != null && guids.Contains((Guid)h.CryptoId))
            .ToArray();

        return selection;
    }

    public async Task<IEnumerable<CryptoCurrency>> GetSandboxCurrencies(CancellationToken cancellationToken = default)
    {
        var guids = await GetSandboxCurrenciesGuids(cancellationToken);

        return await _db.CryptoCurrencies
            .Where(c => guids.Contains(c.Id))
            .ToListAsync(cancellationToken);
    }

    private async Task<IEnumerable<Guid>> GetSandboxCurrenciesGuids(CancellationToken cancellationToken = default)
    {
        var guids = await _appConfigService.GetAppConfigAsync(AppConfigs.SANDBOX_CURRENCIES, Array.Empty<Guid>(), cancellationToken);

        if (guids == null || guids.Length == 0)
            throw new NotFoundException("No sandbox currencies found. Please setup which crypto currencies to use first.");

        return guids;
    }
}
