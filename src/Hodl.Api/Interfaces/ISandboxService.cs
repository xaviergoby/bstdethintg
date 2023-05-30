namespace Hodl.Api.Interfaces;

public interface ISandboxService
{
    Task<IEnumerable<Holding>> GetSandboxHoldings(Guid fundId, CancellationToken cancellationToken = default);

    Task<IEnumerable<CryptoCurrency>> GetSandboxCurrencies(CancellationToken cancellationToken = default);
}
