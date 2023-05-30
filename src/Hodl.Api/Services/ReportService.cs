using Hodl.Api.ViewModels.ReportModels;

namespace Hodl.Api.Services;


public class ReportService : IReportService
{
    private readonly HodlDbContext _db;
    private readonly IFundService _fundService;
    private readonly IMapper _mapper;

    public ReportService(
        HodlDbContext dbContext,
        IFundService fundService,
        IMapper mapper)
    {
        _db = dbContext;
        _fundService = fundService;
        _mapper = mapper;
    }

    /// <summary>
    /// This method is meant for getting 
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="bookingPeriod"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<ReportFundViewModel> GetReportInternal(Guid fundId, string bookingPeriod, CancellationToken cancellationToken = default)
    {
        var fund = await _db.Funds
            .AsNoTracking()
            .Where(f => f.Id == fundId)
            .Include(f => f.FundOwner)
            .Include(f => f.Layers.OrderBy(l => l.LayerIndex))
            .Include(f => f.Holdings.Where(h => h.BookingPeriod == bookingPeriod))
            .ThenInclude(h => h.Currency)
            .Include(f => f.Holdings.Where(h => h.BookingPeriod == bookingPeriod))
            .ThenInclude(h => h.CryptoCurrency)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken) ??
            throw new NotFoundException("Fund not found");

        var fundReport = _mapper.Map<ReportFundViewModel>(fund);

        // And NAV to the fund
        fundReport.Nav = _mapper.Map<ReportPeriodNavViewModel>(await _db.Navs
            .Where(n => n.FundId == fundId && n.Type == NavType.Period && n.BookingPeriod == bookingPeriod)
            .Include(n => n.CurrencyRate)
            .OrderByDescending(n => n.DateTime)
            .AsSingleQuery()
            .FirstOrDefaultAsync(cancellationToken));

        if (fundReport.Nav == null)
            throw new NotFoundException("No report found for the requested fund and booking period.");

        // Calculate the hodling Euro values
        foreach (var holdingView in fundReport.Holdings)
        {
            holdingView.NavUSDValue = holdingView.EndUSDPrice * holdingView.NavBalance;
            holdingView.NavBTCValue = holdingView.EndBTCPrice * holdingView.NavBalance;
            holdingView.EndUSDValue = holdingView.EndUSDPrice * holdingView.EndBalance;
            holdingView.EndBTCValue = holdingView.EndBTCPrice * holdingView.EndBalance;
            holdingView.EndReportingCurrencyValue = holdingView.EndUSDValue / fundReport.Nav.CurrencyRate.USDRate;
            holdingView.LayerName = fundReport.Layers.FirstOrDefault(l => l.LayerIndex == holdingView.LayerIndex)?.Name;
        }

        // Calc layer distribution
        var layerDistribution = await _fundService.CalcLayerDistribution(fund.Holdings, cancellationToken);
        foreach (var layer in fundReport.Layers)
        {
            if (layerDistribution.TryGetValue(layer.LayerIndex, out SumRecord rec))
            {
                layer.CurrentPercentage = rec.TotalSharePercentage;
                layer.NumberOfHoldings = rec.NumberOfItems;
            }
        }

        // Get the trades and transfers
        var holdingIds = fund.Holdings.Select(h => h.Id).ToArray();
        var transfers = await _db.Transfers
            .Where(t => holdingIds.Contains(t.HoldingId))
            .Include(t => t.Holding)
            .ThenInclude(h => h.Currency)
            .Include(t => t.Holding)
            .ThenInclude(h => h.CryptoCurrency)
            .Include(t => t.FeeHolding)
            .ThenInclude(h => h.Currency)
            .Include(t => t.FeeHolding)
            .ThenInclude(h => h.CryptoCurrency)
            .OrderBy(t => t.DateTime)
            .AsSingleQuery()
            .ToArrayAsync(cancellationToken);
        var trades = await _db.Trades
            .Where(t => t.BookingPeriod == bookingPeriod && t.Order.OrderFundings.Any(f => f.FundId == fundId && f.OrderAmount != 0))
            .Include(t => t.Order)
            .ThenInclude(o => o.ExchangeAccount)
            .ThenInclude(a => a.Exchange)
            .Include(t => t.Order)
            .ThenInclude(o => o.OrderFundings.Where(f => f.FundId == fundId && f.OrderAmount != 0))
            .Include(t => t.Order)
            .ThenInclude(o => o.BaseAsset)
            .Include(t => t.Order)
            .ThenInclude(o => o.QuoteAsset)
            .Include(t => t.FeeCurrency)
            .AsSplitQuery()
            .ToArrayAsync(cancellationToken);

        fundReport.TransferLog = transfers.Select(t => _mapper.Map<ReportTransferListViewModel>(t));
        fundReport.TradeLog = GetTradeLog(fund.Id, trades);
        fundReport.TradeSummary = GetTradeSummary(fund, transfers, trades);
        // Filter the holdings last so the summary can be calculated over all
        // holdings involved. Only for the output we can filter holdings with
        // no balances at all.
        fundReport.Holdings = fundReport.Holdings
            .Where(h => h.StartBalance != 0 || h.NavBalance != 0 || h.EndBalance != 0)
            .OrderBy(h => h.CurrencyName)
            .ToArray();

        return fundReport;
    }

    private ReportOrderLogViewModel[] GetTradeLog(Guid fundId, Trade[] trades)
    {
        Dictionary<Guid, ReportOrderLogViewModel> orders = new();
        Dictionary<Guid, Dictionary<Guid, ReportFeeSumViewModel>> orderFees = new();

        foreach (var trade in trades)
        {
            if (!orders.ContainsKey(trade.OrderId))
            {
                var newOrder = _mapper.Map<ReportOrderLogViewModel>(trade.Order);

                newOrder.FundingPercentage = trade.Order.OrderFundings.FirstOrDefault(of => of.FundId == fundId)?.OrderPercentage ?? 0;
                newOrder.Trades.Clear();// Reset because the automapper adds a trade already

                orders[trade.OrderId] = newOrder;
                orderFees[trade.OrderId] = new();
            }
            var order = orders[trade.OrderId];

            order.FilledAmount += trade.Executed;
            order.FilledTotal += trade.Total;
            order.Trades.Add(_mapper.Map<ReportTradeLogViewModel>(trade));

            if (orderFees[trade.OrderId].ContainsKey(trade.FeeCurrencyId))
            {
                orderFees[trade.OrderId][trade.FeeCurrencyId].TotalFee += trade.Fee;
            }
            else
            {
                orderFees[trade.OrderId][trade.FeeCurrencyId] = new ReportFeeSumViewModel()
                {
                    TotalFee = trade.Fee,
                    FeeCryptoSymbol = trade.FeeCurrency?.Symbol ?? "Undefined",
                    FeeCryptoName = trade.FeeCurrency?.Name ?? "Undefined"
                };
            }
        }

        foreach (var order in orders)
        {
            order.Value.TotalFees = orderFees[order.Key].Values;
            order.Value.FundAmount = order.Value.FilledAmount / 100 * order.Value.FundingPercentage;
            order.Value.FundTotal = order.Value.FilledTotal / 100 * order.Value.FundingPercentage;
            order.Value.FundFees = orderFees[order.Key].Values.Select(feesum => new ReportFeeSumViewModel()
            {
                TotalFee = feesum.TotalFee / 100 * order.Value.FundingPercentage,
                FeeCryptoSymbol = feesum.FeeCryptoSymbol,
                FeeCryptoName = feesum.FeeCryptoName
            }).ToArray();
        }

        return orders.Values.Where(o => o.FundingPercentage > 0).ToArray();
    }

    /// <summary>
    /// Get Trade summary dataset. Also includes the transfers transactions. 
    /// All the money movements are summarized and displayed in four 
    /// categories: inflow/outflow, trades in/out, staking summary and fees.
    /// </summary>
    public ReportTradeSummaryViewModel[] GetTradeSummary(Fund fund, Transfer[] transfers, Trade[] trades)
    {
        var tradeSums = fund.Holdings
            .Select(h => _mapper.Map<ReportTradeSummaryViewModel>(h))
            .OrderBy(h => h.CurrencyName)
            .ToArray();

        // Now walk the transfers and add the numbers
        foreach (var transfer in transfers)
        {
            var holding = tradeSums.Single(h => h.Id == transfer.HoldingId);

            switch (transfer.TransactionType)
            {
                case TransactionType.Broker:
                case TransactionType.Transfer:
                    // Add or remove from holding
                    if (transfer.Direction == TransferDirection.In)
                    {
                        holding.TradeSum += transfer.TransferAmount;
                    }
                    else
                    {
                        holding.TradeSum -= transfer.TransferAmount;
                    }
                    break;
                case TransactionType.Inflow:
                case TransactionType.Outflow:
                    // Add or remove from holding
                    if (transfer.Direction == TransferDirection.In)
                    {
                        holding.InOutFlow += transfer.TransferAmount;
                        holding.InOutFlowShares += transfer.Shares;
                    }
                    else
                    {
                        holding.InOutFlow -= transfer.TransferAmount;
                        holding.InOutFlowShares -= transfer.Shares;
                    }
                    break;
                case TransactionType.Reward:
                    // Add to holding
                    holding.StakingRewards += transfer.TransferAmount;
                    break;
                case TransactionType.PerformanceFee:
                case TransactionType.AdministrationFee:
                    holding.Fees += transfer.TransferAmount;
                    break;
                case TransactionType.Profit:
                case TransactionType.Loss:
                    // Add or remove from holding
                    if (transfer.Direction == TransferDirection.In)
                    {
                        holding.ProfitAndLoss += transfer.TransferAmount;
                    }
                    else
                    {
                        holding.ProfitAndLoss -= transfer.TransferAmount;
                    }
                    break;
            }
            // And add the transfer fee
            var feeHolding = tradeSums.Single(h => h.Id == transfer.FeeHoldingId);
            feeHolding.TradeSum -= transfer.TransferFee;
        }

        // And calculate all the trades
        foreach (var trade in trades)
        {
            var fromHolding = tradeSums.Single(h => h.Id == fund.Holdings.Single(h => h.CryptoId == trade.Order.QuoteAssetId).Id);
            var toHolding = tradeSums.Single(h => h.Id == fund.Holdings.Single(h => h.CryptoId == trade.Order.BaseAssetId).Id);
            var feeHolding = tradeSums.Single(h => h.Id == fund.Holdings.Single(h => h.CryptoId == trade.FeeCurrencyId).Id);

            var fundingPercentage = trade.Order.OrderFundings.SingleOrDefault(of => of.FundId == fund.Id)?.OrderPercentage ?? 0;

            switch (trade.Order.Direction)
            {
                case OrderDirection.Sell:
                    fromHolding.TradeSum += (trade.Total * fundingPercentage / 100);
                    toHolding.TradeSum -= (trade.Executed * fundingPercentage / 100);
                    break;
                default:
                    fromHolding.TradeSum -= (trade.Total * fundingPercentage / 100);
                    toHolding.TradeSum += (trade.Executed * fundingPercentage / 100);
                    break;
            }

            feeHolding.TradeSum -= (trade.Fee * fundingPercentage / 100);
        }

        foreach (var sum in tradeSums)
        {
            sum.CalculatedEndValue = sum.StartBalance + sum.InOutFlow + sum.TradeSum + sum.StakingRewards + sum.ProfitAndLoss - sum.Fees;
        }

        return tradeSums;
    }
}
