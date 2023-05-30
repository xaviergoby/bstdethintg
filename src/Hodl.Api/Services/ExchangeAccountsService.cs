using static Hodl.Api.Utils.Factories.CryptoExchangeFactory;

namespace Hodl.Api.Services;

public class ExchangeAccountsService : IExchangeAccountsService
{
    private const string LOG_WARNING_EXCHANGE_NOT_FOUND = "{timestamp} - Exchange API for {message} not found.";

    private readonly HodlDbContext _db;
    protected readonly ILogger<ExchangeAccountsService> _logger;

    public ExchangeAccountsService(
        HodlDbContext dbContext,
        ILogger<ExchangeAccountsService> logger)
    {
        _db = dbContext;
        _logger = logger;
    }

    public async Task<ExchangeAccount> GetExchangeAccount(Guid exchangeAccountId, CancellationToken cancellationToken = default)
    {
        return await _db.ExchangeAccounts
            .AsNoTracking()
            .Where(ea => ea.Id.Equals(exchangeAccountId))
            .Include(ea => ea.Exchange)
            .Include(ea => ea.ChildAccounts)
            .Include(ea => ea.Wallets)
            .Include(ea => ea.Orders.OrderByDescending(t => t.DateTime).Take(100))
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the exchange accounts with API keys.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IList<ExchangeAccount>> GetExchangeAccountsWithApiKeysAsync(CancellationToken cancellationToken = default)
    {
        return await _db.ExchangeAccounts
            .Where(a => !string.IsNullOrWhiteSpace(a.AccountKey) && !string.IsNullOrWhiteSpace(a.AccountSecret))
            .Include(a => a.Exchange)
            .AsSingleQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WalletBalance>> GetExchangeAccountBalances(Guid exchangeAccountId, CancellationToken cancellationToken = default)
    {
        var wallet = await _db.Wallets
            .SingleOrDefaultAsync(w => w.ExchangeAccountId.Equals(exchangeAccountId), cancellationToken) ??
            throw new RestException(HttpStatusCode.NotFound,
                $"No wallet found for the exchange account with id: {exchangeAccountId}");

        return await _db.WalletBalances
            .Where(b => b.Address.Equals(wallet.Address))
            .ToArrayAsync(cancellationToken);
    }

    public async Task UpdateExchangeAccountBalances(Guid exchangeAccountId, bool isTestEnvironment, CancellationToken cancellationToken = default)
    {
        // Create an Exchange API client for the account and get the balances
        var account = await _db.ExchangeAccounts
            .AsNoTracking()
            .Where(ea => ea.Id.Equals(exchangeAccountId))
            .Include(ea => ea.Exchange)
            .SingleOrDefaultAsync(cancellationToken) ??
            throw new NotFoundException($"Exchange account with Id {exchangeAccountId} not found.");

        try
        {
            var apiClient = GetExchangeClient(account.Exchange, account, isTestEnvironment);

            var exchangeBalances = await apiClient.GetBalances(cancellationToken);

            if (exchangeBalances.Any())
            {
                // Wallet address for exchange account will be exchange.Id:Account.Id
                var wallet = await GetOrCreateAccountWallet(account, cancellationToken);
                var symbols = exchangeBalances.Select(b => b.Asset);
                var cryptos = await _db.CryptoCurrencies
                    .Where(c => symbols.Contains(c.Symbol) && c.Active && !c.IsLocked)
                    .ToArrayAsync(cancellationToken);

                var walletBalances = exchangeBalances
                    .Where(b => cryptos.Any(c => c.Symbol.Equals(b.Asset)))
                    .Select(exchangeBalance => new WalletBalance
                    {
                        Address = wallet.Address,
                        CryptoId = cryptos.Single(c => c.Symbol.Equals(exchangeBalance.Asset)).Id,
                        BlockchainNetworkId = null,
                        Timestamp = exchangeBalance.TimeStamp.UtcDateTime,
                        Balance = exchangeBalance.Total
                    })
                    .ToArray();

                // Now save all the balances on the wallet
                await _db.WalletBalances.AddRangeAsync(walletBalances, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException) { }
        catch (ExchangeApiNotFoundException e)
        {
            _logger.LogWarning(LOG_WARNING_EXCHANGE_NOT_FOUND, DateTime.Now, e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, DateTime.Now, e.Message);
        }
    }

    private static string AccountWalletAddress(ExchangeAccount account) => $"{account.Exchange.Id}:{account.Id}";

    private static string AccountWalletDescription(ExchangeAccount account) => $"Wallet for {account.Exchange.ExchangeName} account with the account key {account.AccountKey}";

    private async Task<Wallet> GetOrCreateAccountWallet(ExchangeAccount account, CancellationToken cancellationToken = default)
    {
        // Create an Exchange API client for the account and get the balances
        return await _db.Wallets
            .FirstOrDefaultAsync(w => w.ExchangeAccountId.Equals(account.Id), cancellationToken) ??
            await CreateAccountWallet(account, cancellationToken);
    }

    private async Task<Wallet> CreateAccountWallet(ExchangeAccount account, CancellationToken cancellationToken = default)
    {
        var newWallet = new Wallet
        {
            Address = AccountWalletAddress(account),
            ExchangeAccountId = account.Id,
            Timestamp = DateTime.UtcNow,
            Description = AccountWalletDescription(account)
        };

        _db.Wallets.Add(newWallet);
        await _db.SaveChangesAsync(cancellationToken);

        return newWallet;
    }
}