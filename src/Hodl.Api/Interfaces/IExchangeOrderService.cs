using Hodl.ExchangeAPI.Models;

namespace Hodl.Api.Interfaces;

public interface IExchangeOrderService
{
    Task<Order> InsertOrUpdateExchangeOrder(Guid accountId, ExchangeOrder exchangeOrder, CancellationToken cancellationToken = default);

    Task<Trade> InsertExchangeTrade(Guid accountId, ExchangeTrade exchangeTrade, CancellationToken cancellationToken = default);

    Task<IEnumerable<Trade>> InsertExchangeTrades(Guid accountId, IEnumerable<ExchangeTrade> exchangeTrades, CancellationToken cancellationToken = default);
}
