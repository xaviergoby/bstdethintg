using Hodl.Api.ViewModels.StatisticsModels;

namespace Hodl.Api.Interfaces;

public interface IStatisticsService
{
    Task SetOrderQuotePrices(Guid baseAssetId, OrderDirection orderDirection = OrderDirection.Buy, CancellationToken cancellationToken = default);

    Task<AssetPriceStats> GetAveragePrices(Guid baseAssetId, OrderDirection orderDirection = OrderDirection.Buy, CancellationToken cancellationToken = default);
}