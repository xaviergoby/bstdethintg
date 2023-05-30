using Hodl.Api.ViewModels.StatisticsModels;

namespace Hodl.Api.Services;

public class StatisticsService : IStatisticsService
{
    private readonly HodlDbContext _db;
    private readonly ICryptoCurrencyService _cryptoService;
    private readonly IBookingPeriodHelper _bookingPeriodHelper;
    private readonly IAppConfigService _appConfigService;

    public StatisticsService(
        HodlDbContext db,
        ICryptoCurrencyService cryptoCurrencyService,
        IAppConfigService appConfigService,
        IBookingPeriodHelper bookingPeriodHelper)
    {
        _db = db;
        _cryptoService = cryptoCurrencyService;
        _appConfigService = appConfigService;
        _bookingPeriodHelper = bookingPeriodHelper;
    }

    /// <summary>
    /// This method is for "updating"/"resettings" the property vals of 
    /// AvgPriceCryptoId, AvgPriceRate & AvgPriceSource of an order.
    /// The AvgPriceCryptoId is the currency where the average price 
    /// calculations are generated in. It can differ from the quote currency of 
    /// the order, that's why the exchange rate from the order quote currency 
    /// to avg price quote currency is stored in the funding records.
    /// This quote currency is a systemwide setting and is stored in the 
    /// appSettings table. The default quote currency is BTC.
    /// 
    /// To calculate the exchange rate from the order quote currency to the 
    /// average price quote currency, the following strategy is used:
    /// 1. Is order quote same as avg quote > rate = 1
    /// 2. Can we find an order that transfered order quote with to fund this
    ///    order? > Take that rate currency rate.
    /// 3. Take the exchange rate from the database using the closest listings.
    ///    When no listing is available, try to import from a market API.
    /// </summary>
    /// <param name="baseAssetId">Given cryptocurrency for which Buy or Sell orders have been made AKA "Base Cryptocurrency"</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task SetFundingQuotePrice(Order order, Guid quoteAssetId, CancellationToken cancellationToken = default)
    {
        // Order quoteId same as avg quoteId
        if (order.QuoteAssetId.Equals(quoteAssetId))
        {
            order.AvgPriceCryptoId = quoteAssetId;
            order.AvgPriceRate = 1;
            order.AvgPriceSource = "Order";
            return;
        }

        // TODO: Find a funding order and calculate that price
        // Is there a previous order that converted the avg quote into the order quote?
        //var fundingOrder = await _db.Orders
        //    .Where(o => o.BaseAssetId.Equals(quoteAssetId) && o.QuoteAssetId.Equals)


        // Last step, find the listing closest to the order date and set the
        // rate from there.
        var listing = await _cryptoService.GetListingByDate(order.QuoteAssetId, order.DateTime, _cryptoService.PreferedListingSource, true, cancellationToken);

        if (quoteAssetId.Equals(_cryptoService.BtcGuid) && !listing.BTCPrice.Equals(0))
        {
            // Store the BTC price from the listing
            order.AvgPriceCryptoId = quoteAssetId;
            order.AvgPriceRate = listing.BTCPrice;
            order.AvgPriceSource = listing.Source;
            return;
        }

        var avgQuoteListing = await _cryptoService.GetListingByDate(quoteAssetId, order.DateTime, _cryptoService.PreferedListingSource, true, cancellationToken);

        // Store the Quote price from the listing
        order.AvgPriceCryptoId = quoteAssetId;
        order.AvgPriceRate = listing.USDPrice / avgQuoteListing.USDPrice;
        order.AvgPriceSource = listing.Source;
    }

    /// <summary>
    /// Selects all the orders where the base currency is the same as the 
    /// requested currency, and the average price currency differs from the 
    /// used quote currency. Then it looks for the conversion rate from the 
    /// quote currency on the order to the everage price quote currency. The 
    /// following 
    /// </summary>
    /// <param name="baseAssetId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task SetOrderQuotePrices(Guid baseAssetId, Guid quoteAssetId, OrderDirection orderDirection, CancellationToken cancellationToken = default)
    {
        // Select the orders that need to be updated on the quote asset
        // exchange rate. 
        // Order by Order.QuoteAssetId and then order date to be able to create 
        // an efficient loop where the historical rates can be fetched once.
        var orders = await _db.Orders
            .Where(o =>
                o.BaseAssetId.Equals(baseAssetId) &&
                o.Direction.Equals(orderDirection) &&
                !o.AvgPriceCryptoId.Equals(quoteAssetId))
            .Include(o => o.OrderFundings)
            .OrderBy(o => o.DateTime)
            .ToListAsync(cancellationToken);

        foreach (var order in orders)
        {
            await SetFundingQuotePrice(order, quoteAssetId, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested) return;
        }
    }

    public async Task SetOrderQuotePrices(Guid baseAssetId, OrderDirection orderDirection = OrderDirection.Buy, CancellationToken cancellationToken = default)
    {
        CryptoCurrency baseCryptoCurrency = await _db.CryptoCurrencies
            .AsNoTracking()
            .Where(c => c.Id == baseAssetId)
            .SingleOrDefaultAsync(cancellationToken) ??
            throw new NotFoundException("Base asset not found");

        Guid quoteAssetId = await _appConfigService.GetAppConfigAsync(AppConfigs.AVG_PRICE_QUOTE_ASSET_ID, _cryptoService.BtcGuid, cancellationToken);
        CryptoCurrency quoteCryptoCurrency = await _db.CryptoCurrencies
            .AsNoTracking()
            .Where(c => c.Id == quoteAssetId)
            .SingleOrDefaultAsync(cancellationToken) ??
            throw new NotFoundException("Quote asset not found");

        // Now make sure the orderfundings all have the conversion rate to the quote asset stored.
        await SetOrderQuotePrices(baseCryptoCurrency.Id, quoteCryptoCurrency.Id, orderDirection, cancellationToken);
    }

    /// <summary>
    /// This method is simply meant for sorting all the order fundings for a 
    /// given base asset, so that the loop to calculate the avg prices can use 
    /// the most efficient way to run over the dataset.
    /// </summary>
    /// <param name="baseAssetId"></param>
    /// <param name="orderDirection"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<IEnumerable<OrderFunding>> SortedOrderFundings(Guid baseAssetId, OrderDirection orderDirection = OrderDirection.Buy, CancellationToken cancellationToken = default)
    {
        var cryptoOrders = await _db.Orders
            .AsNoTracking()
            .Where(o => o.BaseAssetId.Equals(baseAssetId) && o.Direction.Equals(orderDirection) && o.AvgPriceRate > 0)
            .Include(o => o.OrderFundings.Where(of => of.OrderPercentage > 0))
            .ThenInclude(of => of.Fund)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return cryptoOrders
            .SelectMany(o => o.OrderFundings)
            .OrderBy(of => _bookingPeriodHelper.CalcBookingPeriod(new DateTimeOffset(of.Order.DateTime)))
            .ThenBy(of => of.FundId)
            .ToList();
    }

    /// <summary>
    /// Compute the avg entry or exit prices from exchange orders for a given 
    /// base cryptocurrency. The average price is calculated agains a stored 
    /// quote currency, saved in the appsettings or default BTC. The results
    /// give the average buy or sell price per bookingperiod per fund, and a 
    /// total over all orders per fund. 
    /// The average price is calculated by deviding the sum of the ammounts or 
    /// the orders by the total of the costs in the average quote currency. 
    /// This means that if the avg quote currency is BTC and the order quote 
    /// currency is USDT, the total of the order is multiplied by the rate from
    /// USDT to BTC.
    /// </summary>
    /// <param name="baseAssetId">A given cryptocurrency for which Buy or Sell orders have been made AKA "Base Cryptocurrency"</param>
    /// <param name="orderDirection">Determines whether Buy or Sell orders are selected.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<AssetPriceStats> GetAveragePrices(Guid baseAssetId, OrderDirection orderDirection = OrderDirection.Buy, CancellationToken cancellationToken = default)
    {
        CryptoCurrency baseCryptoCurrency = await _cryptoService.GetCryptoCurrencyAsync(baseAssetId, cancellationToken) ??
            throw new NotFoundException("Base asset not found");

        Guid quoteAssetId = await _appConfigService.GetAppConfigAsync(AppConfigs.AVG_PRICE_QUOTE_ASSET_ID, _cryptoService.BtcGuid, cancellationToken);
        CryptoCurrency quoteCryptoCurrency = await _cryptoService.GetCryptoCurrencyAsync(quoteAssetId, cancellationToken) ??
            throw new NotFoundException("Quote asset not found");

        // Now make sure the orderfundings all have the conversion rate to the quote asset stored.
        await SetOrderQuotePrices(baseCryptoCurrency.Id, quoteCryptoCurrency.Id, orderDirection, cancellationToken);

        // Get orders sorted 1st by BookingPeriod then 2nd by FundId
        var sortedOrdersFundings = await SortedOrderFundings(baseAssetId, orderDirection, cancellationToken);

        AssetPriceStats result = new()
        {
            BaseAssetId = baseCryptoCurrency.Id,
            BaseAssetName = baseCryptoCurrency.Name,
            BaseAssetSymbol = baseCryptoCurrency.Symbol,
            QuoteAssetId = quoteCryptoCurrency.Id,
            QuoteAssetName = quoteCryptoCurrency.Name,
            QuoteAssetSymbol = quoteCryptoCurrency.Symbol,
        };

        AssetBookingPeriodAggPriceStats bookingperiodAggOrderFundings = null;
        AssetFundAggPriceStats fundAggOrderFundings = null;

        // Loop over the set of sorted OrderFunding's. This creates the avg
        // prices per bookingperiod.
        foreach (var orderFunding in sortedOrdersFundings)
        {
            var currentBookingPeriod = _bookingPeriodHelper.CalcBookingPeriod(new DateTimeOffset(orderFunding.Order.DateTime));

            if (bookingperiodAggOrderFundings == null || bookingperiodAggOrderFundings.BookingPeriod != currentBookingPeriod)
            {
                fundAggOrderFundings = null;

                bookingperiodAggOrderFundings = new()
                {
                    BookingPeriod = currentBookingPeriod,
                };
                result.BookingPeriodAggPriceStats.Add(bookingperiodAggOrderFundings);
            }

            if (fundAggOrderFundings == null || fundAggOrderFundings.FundId != orderFunding.FundId)
            {
                fundAggOrderFundings = new()
                {
                    FundId = orderFunding.FundId,
                    FundName = orderFunding.Fund.FundName,
                };
                bookingperiodAggOrderFundings.FundAggPriceStats.Add(fundAggOrderFundings);
            }

            fundAggOrderFundings.Amount += orderFunding.OrderAmount;
            fundAggOrderFundings.Total += orderFunding.OrderTotal * orderFunding.Order.AvgPriceRate;
        }

        // And finally calculate the totals over all booking periods.
        result.AssetFundAggPriceStats = result.BookingPeriodAggPriceStats
            .SelectMany(x => x.FundAggPriceStats)
            .ToList()
            .GroupBy(k => k.FundId)
            .Select(c => new AssetFundAggPriceStats()
            {
                FundId = c.Key,
                FundName = c.Select(f => f.FundName).FirstOrDefault(),
                Amount = c.Select(n => n.Amount).Sum(),
                Total = c.Select(n => n.Total).Sum(),
            })
            .ToList();

        return result;
    }
}


