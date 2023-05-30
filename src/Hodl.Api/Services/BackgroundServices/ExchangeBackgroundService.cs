using Hodl.ExchangeAPI.Interfaces;
using Microsoft.Extensions.Options;
using static Hodl.Api.Utils.Factories.CryptoExchangeFactory;

namespace Hodl.Api.Services.BackgroundServices;

/// <summary>
/// This background service runs periodically to pick up registered exchange 
/// accounts, and registers event listeners to the supported exchanges to 
/// listen to order and trade events. It then adds the orders and trades in the
/// database.
/// </summary>
public class ExchangeBackgroundService : BackgroundRoutineService
{
    private const string LOG_INFO_ACCOUNT_REGISTERED = "{timestamp} - API client for exchange {exchange} on account id {account} registered";
    private const string LOG_INFO_ACCOUNT_REMOVED = "{timestamp} - API client for account id {account} unregistered";
    private const string LOG_WARNING_EXCHANGE_NOT_FOUND = "{timestamp} - Exchange API for {message} not found.";

    private DateTime _lastUpdate = DateTime.MinValue;
    private readonly int _updateInterval = 300;
    private readonly bool _isTestEnvironment = true;
    private readonly Dictionary<Guid, IBaseExchangeAPI> _apiConnections = new();

    [Serializable]
    private struct AccountUpdate
    {
        public DateTime LastCheckRun;
        public DateTime LastOrderDate;
        public DateTime LastTradeDate;

        public AccountUpdate()
        {
            LastCheckRun = DateTime.Now;
            LastOrderDate = DateTime.MinValue;
            LastTradeDate = DateTime.MinValue;
        }

        public override string ToString() => LastOrderDate.ToString();
    }

    public ExchangeBackgroundService(
        IOptions<AppDefaults> settings,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ExchangeBackgroundService> logger)
        : base(serviceScopeFactory, logger)
    {
        _updateInterval = settings.Value.ExchangeTradeEventsUpdateInSeconds;
        _isTestEnvironment = settings.Value.IsTestEnvironment();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await StartLoopAsync(UpdateTradeEventListenersCheckAndRun, cancellationToken);
    }

    /// <summary>
    /// Checks if update is needed. When the process must run and a lock is 
    /// granted, the AddLatestListings is triggered.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<TimeSpan> UpdateTradeEventListenersCheckAndRun(IServiceScope scope, CancellationToken cancellationToken)
    {
        // Check and lock for updating the event registrations
        // Flow:
        // 1. Check last update timestamp, if longer ago than interval then proceed
        // 2. Register and unregister the API event listeners
        // 3. Update last update timestamp

        DateTime utcNow = DateTime.UtcNow;

        // There is no need for a lock system as all the running services can
        // register event listeners. This makes the system even more stable in
        // handling the order and trade events, since they can all pick up the
        // events. When one service is down, other running services do still
        // pick up the changes.
        // In the handling of the events, the orders must be checked against
        // the database before adding or updating.
        if (CheckTimeoutExpired(_lastUpdate, utcNow, _updateInterval))
        {
            if (await CheckExchangeAccounts(scope, cancellationToken))
            {
                _lastUpdate = utcNow;
            }
        }

        return TimeSpan.FromSeconds(Math.Max(30, _updateInterval - DateTime.UtcNow.Subtract(_lastUpdate).TotalSeconds));
    }

    /// <summary>
    /// Get all the accounts with API keys and check them for new orders and 
    /// trades. On every run, check the accounts and register- or unregister 
    /// the Exchange API clients to listen to order and trades changes from 
    /// the exchanges.
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<bool> CheckExchangeAccounts(IServiceScope scope, CancellationToken cancellationToken)
    {
        var appConfigService = scope.ServiceProvider.GetRequiredService<IAppConfigService>();
        var exchangeAccountsService = scope.ServiceProvider.GetRequiredService<IExchangeAccountsService>();

        // Get the Exchange accounts to check the orders on and execute the
        // process of retrieving the orders and trades and save them in the
        // database.
        var accounts = await exchangeAccountsService.GetExchangeAccountsWithApiKeysAsync(cancellationToken);

        // Remove exchange API connections for accounts that are removed.
        var removedAccounts = _apiConnections.Keys.Where(k => !accounts.Any(a => a.Id == k)).ToArray();
        foreach (var accountId in removedAccounts)
        {
            _apiConnections.Remove(accountId);

            _logger.LogInformation(LOG_INFO_ACCOUNT_REMOVED, DateTime.Now, accountId);
        }
        GC.Collect();

        foreach (var account in accounts)
        {
            try
            {
                // Create an Exchange API client for all exchange accounts and
                // register the event listeners.

                // When the account exists, but the key is modified, remove the old connection
                if (_apiConnections.ContainsKey(account.Id) && _apiConnections[account.Id].ApiKey != account.AccountKey)
                {
                    if (_apiConnections[account.Id] is IDisposable d) d.Dispose();

                    _apiConnections.Remove(account.Id);
                }

                // Create connection for the account that is not yet in the dictionary
                if (!_apiConnections.ContainsKey(account.Id))
                {
                    // Add the api connection
                    _apiConnections[account.Id] = GetExchangeClient(account.Exchange, account, _isTestEnvironment);
                    // Add event handlers
                    _apiConnections[account.Id].NewOrder += DoOnUpdateOrder;
                    _apiConnections[account.Id].UpdateOrder += DoOnUpdateOrder;
                    _apiConnections[account.Id].NewTrade += DoOnNewTrade;
                    // And activate the API stream subscription
                    await _apiConnections[account.Id].SubscribeOrderUpdates(cancellationToken);

                    _logger.LogInformation(LOG_INFO_ACCOUNT_REGISTERED, DateTime.Now, account.Exchange, account);
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

        return true;
    }

    private void DoOnUpdateOrder(object sender, OrderEventArgs orderEvent)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var orderService = GetScopedOrderService(scope);

        Task.Run(async () => await orderService.InsertOrUpdateExchangeOrder(orderEvent.AccountId, orderEvent.Order, new CancellationToken())).Wait();
    }

    private void DoOnNewTrade(object sender, TradeEventArgs tradeEvent)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var orderService = GetScopedOrderService(scope);

        Task.Run(async () => await orderService.InsertExchangeTrade(tradeEvent.AccountId, tradeEvent.Trade, new CancellationToken())).Wait();
    }

    private static IExchangeOrderService GetScopedOrderService(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IExchangeOrderService>();
}
