using Hodl.Api.Utils.Factories;
using Hodl.Api.ViewModels.TradingModels;
using Microsoft.Extensions.Options;

namespace Hodl.Api.Controllers.Trading;

[ApiController]
[Route("orders")]
public class OrderController : BaseController
{
    private const int PAST_MONTHS_IMPORT_ORDERS = 6;// Number of months back to retrieve orders from exchange to import

    private readonly IOrderService _orderService;
    private readonly IExchangeOrderService _exchangeOrderService;
    private readonly IExchangeAccountsService _exchangeAccountsService;

    private readonly bool _isTestEnvironment = true;

    public OrderController(
        IOrderService orderService,
        IExchangeOrderService exchangeOrderService,
        IExchangeAccountsService exchangeAccountsService,
        IOptions<AppDefaults> settings,
        IMapper mapper,
        ILogger<OrderController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _orderService = orderService;
        _exchangeOrderService = exchangeOrderService;
        _exchangeAccountsService = exchangeAccountsService;
        _isTestEnvironment = settings.Value.IsTestEnvironment();
    }

    #region Orders

    /// <summary>
    /// Gets a paged list of orders with overview properties.
    /// </summary>
    /// <param name="page">Page index (optional)</param>
    /// <param name="itemsPerPage">Number of items per page (optional, default 20)</param>
    /// <returns></returns>
    [HttpGet]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<PagingViewModel<OrderListViewModel>>> Get(
        string orderNumber,
        string cryptoSymbols,
        string walletAddres,
        OrderDirection? direction,
        OrderState? state,
        DateTime? fromDateTime,
        DateTime? toDateTime,
        bool filterFunded,
        int page, int? itemsPerPage,
        CancellationToken ct)
    {
        // Create a paged resultset & paged view model instance with the queried data
        var pageResult = await _orderService.GetOrders(
            orderNumber,
            cryptoSymbols,
            walletAddres,
            direction,
            state,
            fromDateTime,
            toDateTime,
            filterFunded,
            page, itemsPerPage,
            ct);

        var pageResultView = _mapper.Map<PagingViewModel<OrderListViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    [HttpGet]
    [Route("{orderId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<OrderDetailViewModel>> GetOrder(Guid orderId, CancellationToken ct)
    {
        var order = await _orderService.GetOrder(orderId, ct);

        if (order == null)
            return NotFound();

        var orderModel = _mapper.Map<OrderDetailViewModel>(order);

        return Ok(orderModel);
    }

    /// <summary>
    /// Add a new Order.
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpPost]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<OrderListViewModel>> PostOrder([FromBody] OrderAddViewModel order, bool overrideBalanceCheck = true)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        // Prepare for saving. When an empty string is added, still a foreign
        // key reference error pops up.
        if (string.IsNullOrEmpty(order.WalletAddress))
            order.WalletAddress = null;

        var inputOrder = _mapper.Map<Order>(order);
        var newOrder = await _orderService.InsertOrder(inputOrder, overrideBalanceCheck, true);
        var newOrderModel = _mapper.Map<OrderListViewModel>(newOrder);

        return Ok(newOrderModel);
    }

    [HttpPut]
    [Route("{orderId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> PutOrder(Guid orderId, [FromBody] OrderEditViewModel order)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var updatedOrder = _mapper.Map<Order>(order);
        _ = await _orderService.UpdateOrder(orderId, updatedOrder, true);

        return Ok();
    }

    [HttpDelete]
    [Route("{orderId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteOrder(Guid orderId)
    {
        await _orderService.DeleteOrder(orderId);

        return Ok();
    }

    #endregion

    #region Trades

    /// <summary>
    /// Gets a paged list of trades on the order.
    /// </summary>
    /// <param name="page">Page index (optional)</param>
    /// <param name="itemsPerPage">Number of items per page (optional, default 20)</param>
    /// <returns></returns>
    [HttpGet]
    [Route("{orderId}/trades")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<PagingViewModel<TradeListViewModel>>> GetTrades(Guid orderId, int page, int? itemsPerPage, CancellationToken ct)
    {
        // Create a paged resultset & paged view model instance with the queried data
        var pageResult = await _orderService.GetTrades(orderId, page, itemsPerPage, ct);

        var pageResultView = _mapper.Map<PagingViewModel<TradeListViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    [HttpGet]
    [Route("{orderId}/trades/{tradeId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<TradeDetailViewModel>> GetTrade(Guid orderId, Guid tradeId, CancellationToken ct)
    {
        var trade = await _orderService.GetTrade(orderId, tradeId, ct);

        if (trade == null)
            return NotFound();

        var tradeModel = _mapper.Map<TradeDetailViewModel>(trade);

        return Ok(tradeModel);
    }

    /// <summary>
    /// Add a new Order.
    /// </summary>
    /// <param name="trade"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpPost]
    [Route("{orderId}/trades/")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<TradeListViewModel>> PostTrade(Guid orderId, [FromBody] TradeAddViewModel trade)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var newTrade = await _orderService.InsertTrade(orderId, _mapper.Map<Trade>(trade));
        var newTradeModel = _mapper.Map<TradeListViewModel>(newTrade);

        return Ok(newTradeModel);
    }

    [HttpDelete]
    [Route("{orderId}/trades/{tradeId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeleteTrade(Guid orderId, Guid tradeId)
    {
        await _orderService.DeleteTrade(orderId, tradeId);

        return Ok();
    }

    #endregion

    #region TradePairs

    /// <summary>
    /// Gets a paged list of orders with overview properties.
    /// </summary>
    /// <param name="page">Page index (optional)</param>
    /// <param name="itemsPerPage">Number of items per page (optional, default 20)</param>
    /// <returns></returns>
    [HttpGet]
    [Route("/pairs")]
    public async Task<ActionResult<PagingViewModel<PairViewModel>>> GetPairs(
        Guid cryptoId, int page, int? itemsPerPage, CancellationToken ct)
    {
        // Create a paged resultset
        var pageResult = await _orderService.GetPairs(cryptoId, page, itemsPerPage, ct);
        var pageResultView = _mapper.Map<PagingViewModel<PairViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    [HttpGet]
    [Route("/pairs/search")]
    public async Task<ActionResult<PagingViewModel<PairViewModel>>> FindPairs(
        string pairstring, int page, int? itemsPerPage, CancellationToken ct)
    {
        // Create a paged resultset
        var pageResult = await _orderService.FindPairs(pairstring, page, itemsPerPage, ct);
        var pageResultView = _mapper.Map<PagingViewModel<PairViewModel>>(pageResult);

        return Ok(pageResultView);
    }

    [HttpDelete]
    [Route("/pairs/{pairstring}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DeletePair(string pairstring)
    {
        await _orderService.DeletePair(pairstring);

        return Ok();
    }

    #endregion

    #region Exchanges
    /// <summary>
    /// Get Exchange orders for an exchange account and order pair.
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpGet]
    [Route("exchange/{exchangeAccountId}")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<ExchangeOrderListViewModel[]>> GetExchangeOrders(Guid exchangeAccountId, string pair, CancellationToken ct)
    {
        var exchangeAccount = await _exchangeAccountsService.GetExchangeAccount(exchangeAccountId, ct);

        if (exchangeAccount == null)
            return NotFound($"Exchange account with id {exchangeAccountId} not found");

        try
        {
            var exchangeApiClient = CryptoExchangeFactory.GetExchangeClient(exchangeAccount.Exchange, exchangeAccount, _isTestEnvironment);

            // Use startDateTime max 2 months ago. Longer should not be needed because the NAV is too far back. 
            var exchangeOrders = await exchangeApiClient.GetOrders(pair, DateTime.UtcNow.AddMonths(-PAST_MONTHS_IMPORT_ORDERS).Date, ct);

            // Now try to find the orders in the database to show if the orders are already imported before
            var localOrders = await _orderService.GetOrderByExchangeOrderIdList(exchangeAccount.Id, exchangeOrders.Select(o => o.OrderId).ToArray(), ct);

            return Ok(exchangeOrders
                .Select(o =>
                {
                    var dto = _mapper.Map<ExchangeOrderListViewModel>(o);
                    dto.InternalOrderId = localOrders.SingleOrDefault(o => o.ExchangeOrderId.Equals(dto.OrderId))?.Id;
                    return dto;
                })
                .ToArray());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get Exchange orders for an exchange account and order pair.
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    /// <exception cref="RestException"></exception>
    [HttpPost]
    [Route("exchange/{exchangeAccountId}/import")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<ActionResult<OrderListViewModel>> ImportExchangeOrder(Guid exchangeAccountId, [FromBody] ExchangeOrderListViewModel importOrder, CancellationToken ct)
    {
        var exchangeAccount = await _exchangeAccountsService.GetExchangeAccount(exchangeAccountId, ct);

        if (exchangeAccount == null)
            return NotFound($"Exchange account with id {exchangeAccountId} not found");

        _logger.LogInformation("Importing order {orderid} from exchange {exchange}.", importOrder.OrderId, exchangeAccount.Exchange.ExchangeName);

        try
        {
            var exchangeApiClient = CryptoExchangeFactory.GetExchangeClient(exchangeAccount.Exchange, exchangeAccount, _isTestEnvironment);

            // Get the exchange order insert that one and then insert all the trades
            var exchangeOrder = await exchangeApiClient.GetOrder($"{importOrder.BaseAsset}/{importOrder.QuoteAsset}", importOrder.OrderId, ct);

            var order = await _exchangeOrderService.InsertOrUpdateExchangeOrder(exchangeAccount.Id, exchangeOrder, ct);

            // Now import all the trades
            var exchangeTrades = await exchangeApiClient.GetOrderTrades($"{importOrder.BaseAsset}/{importOrder.QuoteAsset}", importOrder.OrderId, ct);
            var tradeCount = await _exchangeOrderService.InsertExchangeTrades(exchangeAccount.Id, exchangeTrades, ct);

            _logger.LogInformation("Import order {orderid} from exchange {exchange} success. Added order {ordernumber} with {tradeCount} trades.",
                importOrder.OrderId, exchangeAccount.Exchange.ExchangeName, order.OrderNumber, tradeCount);

            return Ok(_mapper.Map<OrderListViewModel>(order));
        }
        catch (Exception e)
        {
            _logger.LogInformation("Import order {orderid} from exchange {exchange} failed.", importOrder.OrderId, exchangeAccount.Exchange.ExchangeName);
            _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, DateTime.Now, e.Message);

            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = e.Message
            });
        }
    }

    #endregion
}
