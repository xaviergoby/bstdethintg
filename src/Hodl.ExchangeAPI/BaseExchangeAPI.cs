using Hodl.ExchangeAPI.Interfaces;
using Hodl.ExchangeAPI.Models;

namespace Hodl.ExchangeAPI;

public abstract class BaseExchangeAPI : IDisposable, IBaseExchangeAPI
{
    private readonly Guid _accountId;
    private readonly string _apiKey;

    public event EventHandler<OrderEventArgs> NewOrder;
    public event EventHandler<OrderEventArgs> UpdateOrder;
    public event EventHandler<TradeEventArgs> NewTrade;

    public Guid AccountId { get => _accountId; }

    public BaseExchangeAPI(Guid accountId, string apiKey)
    {
        _accountId = accountId;
        _apiKey = apiKey;
    }

    #region Dispose
    protected bool _disposedValue;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            _disposedValue = true;
        }
    }
    #endregion

    #region Properties
    public string ApiKey { get => _apiKey; }
    #endregion

    #region Events
    protected virtual void OnNewOrder(ExchangeOrder order)
    {
        NewOrder?.Invoke(this, new()
        {
            AccountId = _accountId,
            Order = order
        }); ;
    }

    protected virtual void OnUpdateOrder(ExchangeOrder order)
    {
        UpdateOrder?.Invoke(this, new()
        {
            AccountId = _accountId,
            Order = order
        });
    }

    protected virtual void OnNewTrade(ExchangeTrade trade)
    {
        NewTrade?.Invoke(this, new()
        {
            AccountId = _accountId,
            Trade = trade
        });
    }
    #endregion

    public abstract Task<bool> SubscribeOrderUpdates(CancellationToken cancelationToken = default);

    public abstract Task<IEnumerable<ExchangeOrder>> GetOrders(string symbol, DateTime startTime, CancellationToken cancelationToken = default);

    public abstract Task<ExchangeOrder> GetOrder(string symbol, string orderId, CancellationToken cancelationToken = default);

    public abstract Task<IEnumerable<ExchangeTrade>> GetOrderTrades(string symbol, string orderId, CancellationToken cancelationToken = default);

    public abstract Task<IEnumerable<ExchangeBalance>> GetBalances(CancellationToken cancelationToken = default);
}
