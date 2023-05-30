namespace Hodl.Api.Interfaces;

public interface IOrderService
{
    Task<PagingModel<Order>> GetOrders(
        string orderNumber,
        string cryptoSymbols,
        string walletAddres,
        OrderDirection? direction,
        OrderState? state,
        DateTime? fromDateTime,
        DateTime? toDateTime,
        bool filterFunded,
        int page, int? itemsPerPage,
        CancellationToken cancellationToken = default);

    Task<Order> GetOrder(Guid orderId, CancellationToken cancellationToken = default);

    Task<Order> GetOrderByExchangeOrderId(Guid exchangeAccountId, string exchangeOrderId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Order>> GetOrderByExchangeOrderIdList(Guid exchangeAccountId, string[] exchangeOrderIds, CancellationToken cancellationToken = default);

    Task<Order> InsertOrder(Order newOrder, bool overrideBalanceCheck, bool requireFundings, CancellationToken cancellationToken = default);

    Task<Order> UpdateOrder(Guid orderId, Order order, bool requireFundings, CancellationToken cancellationToken = default);

    Task<bool> DeleteOrder(Guid orderId, CancellationToken cancellationToken = default);

    Task<PagingModel<Trade>> GetTrades(Guid orderId, int page, int? itemsPerPage, CancellationToken cancellationToken = default);

    Task<Trade> GetTrade(Guid orderId, Guid tradeId, CancellationToken cancellationToken = default);

    Task<Trade> GetTradeByExchangeTradeId(Guid exchangeAccountId, string exchangeOrderId, string transactionId, CancellationToken cancellationToken = default);

    Task<Trade> InsertTrade(Guid orderId, Trade trade, bool checkOrderState = true, CancellationToken cancellationToken = default);

    Task<IEnumerable<Trade>> InsertTrades(Guid orderId, IEnumerable<Trade> trades, bool checkOrderState = true, CancellationToken cancellationToken = default);

    Task<bool> DeleteTrade(Guid orderId, Guid tradeId, CancellationToken cancellationToken = default);

    Task<PagingModel<Pair>> GetPairs(Guid cryptoId, int page, int? itemsPerPage, CancellationToken cancellationToken = default);

    Task<PagingModel<Pair>> FindPairs(string pairstring, int page, int? itemsPerPage, CancellationToken cancellationToken = default);

    Task<bool> DeletePair(string pairstring, CancellationToken cancellationToken = default);
}
