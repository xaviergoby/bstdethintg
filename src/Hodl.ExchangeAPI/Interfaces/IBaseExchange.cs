using Hodl.ExchangeAPI.Models;

namespace Hodl.ExchangeAPI.Interfaces;

public interface IBaseExchangeAPI
{
    string ApiKey { get; }

    event EventHandler<OrderEventArgs> NewOrder;
    event EventHandler<OrderEventArgs> UpdateOrder;
    event EventHandler<TradeEventArgs> NewTrade;

    Task<bool> SubscribeOrderUpdates(CancellationToken cancelationToken = default);

    Task<IEnumerable<ExchangeOrder>> GetOrders(string symbol, DateTime startTime, CancellationToken cancelationToken = default);

    Task<ExchangeOrder> GetOrder(string symbol, string orderId, CancellationToken cancelationToken = default);

    Task<IEnumerable<ExchangeTrade>> GetOrderTrades(string symbol, string orderId, CancellationToken cancelationToken = default);

    Task<IEnumerable<ExchangeBalance>> GetBalances(CancellationToken cancelationToken = default);

}

public class OrderEventArgs : EventArgs
{
    public Guid AccountId { get; set; }

    public ExchangeOrder Order { get; set; }
}
public class TradeEventArgs : EventArgs
{
    public Guid AccountId { get; set; }

    public ExchangeTrade Trade { get; set; }
}
