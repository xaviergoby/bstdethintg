namespace Hodl.Api.Interfaces;

public interface IWalletService
{
    Task<bool> ImportWalletBalances(Wallet wallet, bool testEnvironment, CancellationToken cancellationToken = default);

    Task<bool> ImportWalletNormalTransactions(Wallet wallet, bool testEnvironment, CancellationToken cancellationToken = default);
}