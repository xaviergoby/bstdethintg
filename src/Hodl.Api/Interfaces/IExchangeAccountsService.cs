namespace Hodl.Api.Interfaces;

public interface IExchangeAccountsService
{
    Task<ExchangeAccount> GetExchangeAccount(Guid exchangeAccountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the exchange accounts with API keys.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IList<ExchangeAccount>> GetExchangeAccountsWithApiKeysAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<WalletBalance>> GetExchangeAccountBalances(Guid exchangeAccountId, CancellationToken cancellationToken = default);

    Task UpdateExchangeAccountBalances(Guid exchangeAccountId, bool isTestEnvironment, CancellationToken cancellationToken = default);
}
