using Hodl.ExplorerAPI;
using Hodl.ExplorerAPI.Models;
using Hodl.MarketAPI.Models;

namespace Hodl.Api.Services;

public class WalletService : IWalletService
{
    private readonly HodlDbContext _db;
    private readonly IServiceProvider _serviceProvider;
    protected readonly ILogger<WalletService> _logger;
    private readonly IMapper _mapper;


    public WalletService(
        HodlDbContext dbContext,
        IServiceProvider serviceProvider,
        ILogger<WalletService> logger,
        IMapper mapper)
    {
        _db = dbContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Method for importing in the database the balances of a wallet for a given address 
    /// </summary>
    /// <param name="wallet"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> ImportWalletBalances(Wallet wallet, bool testEnvironment, CancellationToken cancellationToken)
    {
        var explorers = _serviceProvider.GetServices<IBlockExplorer>();

        var blockChains = await _db.BlockchainNetworks
            .AsNoTracking()
            .Where(b => b.IsTestNet == false)
            .Include(n => n.TokenContracts)
            .ThenInclude(c => c.CryptoCurrency)
            .AsSingleQuery()
            .ToArrayAsync(cancellationToken);
        try
        {
            foreach (var blockchain in blockChains)
            {
                // Skip testnets for production environment
                if (!testEnvironment && blockchain.IsTestNet) continue;

                var explorerBalances = new List<ExplorerBalance>();

                foreach (var explorer in explorers
                    .Where(e => e.SupportsAddress(wallet.Address) && e.SupportsExplorerUrl(blockchain.ExplorerUrl)))
                    try
                    {
                        var tokens = blockchain.TokenContracts.Select(c => new ExplorerAPI.Models.TokenContract()
                        {
                            Address = c.ContractAddress,
                            TokenName = c.CryptoCurrency.Name,
                            TokenSymbol = c.CryptoCurrency.Symbol,
                            Decimals = c.CryptoCurrency.Decimals
                        });

                        var explorerResult = await explorer.GetBalances(blockchain.ExplorerUrl, wallet.Address, tokens, cancellationToken);

                        //explorerBalances.AddRange(explorerResult.Where(b => b.Balance > 0));
                        explorerBalances.AddRange(explorerResult);
                    }
                    catch (NotImplementedException) { }

                if (explorerBalances.Any())
                {
                    var symbols = explorerBalances.Select(b => b.CurrencySymbol);
                    var cryptos = await _db.CryptoCurrencies
                        .Where(c => symbols.Contains(c.Symbol) && c.Active && !c.IsLocked)
                        .ToArrayAsync(cancellationToken);

                    var walletBalances = explorerBalances
                        .Where(b => cryptos.Any(c => c.Symbol.Equals(b.CurrencySymbol)))
                        .Select(explorerBalance => new WalletBalance
                        {
                            Address = wallet.Address,
                            BlockchainNetworkId = blockchain.Id,
                            CryptoId = cryptos.Single(c => c.Symbol.Equals(explorerBalance.CurrencySymbol)).Id,
                            Timestamp = explorerBalance.TimeStamp.UtcDateTime,
                            Balance = explorerBalance.Balance
                        })
                        .ToArray();

                    // Now save all the balances on the wallet
                    await _db.WalletBalances.AddRangeAsync(walletBalances, cancellationToken);
                    await _db.SaveChangesAsync(cancellationToken);
                }
            }

            return true;
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, DateTime.Now, e.Message);
        }

        return false;
    }

    /// <summary>
    /// Method for importing in the database the normal transactions of a wallet for a given (external account AKA not internal/contract) address 
    /// </summary>
    /// <param name="wallet"></param>
    /// <param name="testEnvironment"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> ImportWalletNormalTransactions(Wallet wallet, bool testEnvironment, CancellationToken cancellationToken = default)
    {
        var explorers = _serviceProvider.GetServices<IBlockExplorer>();

        var blockChains = await _db.BlockchainNetworks
            .AsNoTracking()
            .Where(b => b.IsTestNet == false)
            .AsSingleQuery()
            .ToArrayAsync(cancellationToken);
        try
        {
            foreach (var blockchain in blockChains)
            {
                // Skip testnets for production environment
                if (!testEnvironment && blockchain.IsTestNet) continue;

                var explorerTransactions = new List<Transaction>();

                foreach (var explorer in explorers
                    .Where(e => e.SupportsAddress(wallet.Address) && e.SupportsExplorerUrl(blockchain.ExplorerUrl)))
                    try
                    {
                        var explorerResult = await explorer.NormalTransactionsByAddress(blockchain.ExplorerUrl, wallet.Address);

                        explorerTransactions.AddRange(explorerResult);
                    }
                    catch (NotImplementedException) { }

                if (explorerTransactions.Any())
                {

                    var orders = new List<Order>();

                    foreach(var explorerTransaction in explorerTransactions)
                    {
                        var order = _mapper.Map<Order>(explorerTransaction);
                        orders.Add(order);
                    }

                    // Now save all the orders generated from mapping the blockchain transaction data into the order dto
                    await _db.Orders.AddRangeAsync(orders, cancellationToken);
                    await _db.SaveChangesAsync(cancellationToken);
                }
            }

            return true;
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, DateTime.Now, e.Message);
        }

        return false;
    }
}
