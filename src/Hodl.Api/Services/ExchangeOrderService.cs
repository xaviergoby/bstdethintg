using Hodl.ExchangeAPI.Models;

namespace Hodl.Api.Services;

public class ExchangeOrderService : IExchangeOrderService
{
    private const string UPDATE_ORDER_OR_TRADES = "Exchange.Event.OrderUpdate";
    private const int UPDATE_ORDER_OR_TRADES_LOCK_TIMEOUT = 1000;

    private readonly IAppConfigService _appConfigService;
    private readonly IOrderService _orderService;
    private readonly ICryptoCurrencyService _cryptoService;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    public ExchangeOrderService(
        IAppConfigService appConfigService,
        IOrderService orderService,
        ICryptoCurrencyService cryptoCurrencyService,
        IMapper mapper,
        ILogger<OrderService> logger)
    {
        _appConfigService = appConfigService;
        _orderService = orderService;
        _cryptoService = cryptoCurrencyService;
        _mapper = mapper;
        _logger = logger;
    }

    #region ExchangeOrders
    public async Task<Order> InsertOrUpdateExchangeOrder(Guid accountId, ExchangeOrder exchangeOrder, CancellationToken cancellationToken = default)
    {
        // First get a global lock for inserting order/trade
        if (await _appConfigService.WaitForProcessLock(UPDATE_ORDER_OR_TRADES, UPDATE_ORDER_OR_TRADES_LOCK_TIMEOUT, cancellationToken))
        {
            try
            {
                Order order = await _orderService.GetOrderByExchangeOrderId(accountId, exchangeOrder.OrderId, cancellationToken);

                if (order == null)
                {
                    var baseAsset = await _cryptoService.GetCryptoCurrencyBySymbol(exchangeOrder.BaseAsset.Symbol, cancellationToken) ??
                        throw new NotFoundException($"The base asset from the order with ID {exchangeOrder.OrderId} can not be found in the database.");

                    var quoteAsset = await _cryptoService.GetCryptoCurrencyBySymbol(exchangeOrder.QuoteAsset.Symbol, cancellationToken) ??
                        throw new NotFoundException($"The quote asset from the order with ID {exchangeOrder.OrderId} can not be found in the database.");

                    order = _mapper.Map<Order>(exchangeOrder);

                    order.ExchangeAccountId = accountId;
                    order.BaseAssetId = baseAsset.Id;
                    order.QuoteAssetId = quoteAsset.Id;

                    return await _orderService.InsertOrder(order, true, false, cancellationToken);
                }
                else
                {
                    _mapper.Map(exchangeOrder, order);

                    return await _orderService.UpdateOrder(order.Id, order, false, cancellationToken);
                }
            }
            finally
            {
                await _appConfigService.ReleaseProcessLock(UPDATE_ORDER_OR_TRADES, cancellationToken);
            }
        }
        else
        {
            throw new Exception("Could not get lock on insert exchange order process.");
        }
    }

    /// <summary>
    /// Insert a trade. A trade never changes, it can only be added.
    /// </summary>
    /// <param name="exchangeTrade"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Trade> InsertExchangeTrade(Guid accountId, ExchangeTrade exchangeTrade, CancellationToken cancellationToken = default)
    {
        Trade trade = null;

        // First get a global lock for inserting order/trade
        if (await _appConfigService.WaitForProcessLock(UPDATE_ORDER_OR_TRADES, UPDATE_ORDER_OR_TRADES_LOCK_TIMEOUT, cancellationToken))
        {
            try
            {
                var order = await _orderService.GetOrderByExchangeOrderId(accountId, exchangeTrade.OrderId, cancellationToken) ??
                    throw new NotFoundException($"Can not add trade because the exchange order with ID {exchangeTrade.OrderId} can not be found.");
                var feeCurrency = await _cryptoService.GetCryptoCurrencyBySymbol(exchangeTrade.FeeAsset.Symbol, cancellationToken) ??
                    throw new NotFoundException($"The fee currency from the exchange order with ID {exchangeTrade.OrderId} can not be found in the database.");

                // Check if the trade is already in the database, if so, do nothing, otherwise insert the trade
                trade = await _orderService.GetTradeByExchangeTradeId(accountId, exchangeTrade.OrderId, exchangeTrade.Id, cancellationToken);

                if (trade == null)
                {
                    trade = _mapper.Map<Trade>(exchangeTrade);
                    trade.FeeCurrencyId = feeCurrency.Id;

                    return await _orderService.InsertTrade(order.Id, trade, false, cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, DateTime.Now, e.Message);
            }
            finally
            {
                await _appConfigService.ReleaseProcessLock(UPDATE_ORDER_OR_TRADES, cancellationToken);
            }
        }
        return trade;
    }

    /// <summary>
    /// Insert multiple trades at ones. A trade never changes, it can only be added.
    /// All the trades must be from the same order.
    /// </summary>
    /// <param name="exchangeTrade"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Trade>> InsertExchangeTrades(Guid accountId, IEnumerable<ExchangeTrade> exchangeTrades, CancellationToken cancellationToken = default)
    {
        if (exchangeTrades == null || !exchangeTrades.Any()) return null;

        // First get a global lock for inserting order/trade
        if (await _appConfigService.WaitForProcessLock(UPDATE_ORDER_OR_TRADES, UPDATE_ORDER_OR_TRADES_LOCK_TIMEOUT, cancellationToken))
        {
            try
            {
                var exchangeOrderId = exchangeTrades.First().OrderId;
                var order = await _orderService.GetOrderByExchangeOrderId(accountId, exchangeOrderId, cancellationToken) ??
                    throw new NotFoundException($"Can not add trade because the exchange order with ID {exchangeOrderId} can not be found.");
                List<Trade> newTrades = new();

                // Start the loop here
                foreach (var exchangeTrade in exchangeTrades)
                {
                    var feeCurrency = await _cryptoService.GetCryptoCurrencyBySymbol(exchangeTrade.FeeAsset.Symbol, cancellationToken) ??
                        throw new NotFoundException($"The fee currency from the exchange order with ID {exchangeOrderId} can not be found in the database.");

                    // Check if the trade is already in the database, if so, do nothing, otherwise insert the trade
                    Trade trade = await _orderService.GetTradeByExchangeTradeId(accountId, exchangeOrderId, exchangeTrade.Id, cancellationToken);

                    if (trade == null)
                    {
                        trade = _mapper.Map<Trade>(exchangeTrade);
                        trade.FeeCurrencyId = feeCurrency.Id;

                        newTrades.Add(trade);
                    }
                }

                // And now add all the trades at ones
                return await _orderService.InsertTrades(order.Id, newTrades, false, cancellationToken);

            }
            catch (Exception e)
            {
                _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, DateTime.Now, e.Message);
            }
            finally
            {
                await _appConfigService.ReleaseProcessLock(UPDATE_ORDER_OR_TRADES, cancellationToken);
            }
        }

        return null;
    }
    #endregion
}
