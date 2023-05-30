using Hodl.Api.FilterParams;
using Microsoft.EntityFrameworkCore.Storage;
using System.Web;

namespace Hodl.Api.Services;

public class OrderService : IOrderService
{
    private const int DEFAULT_ORDERS_PER_PAGE = 40;
    private const int DEFAULT_TRADES_PER_PAGE = 1000;

    private readonly char[] splitChars = new char[3] { ' ', ',', '/' };

    private readonly HodlDbContext _db;
    private readonly IChangeLogService _changeLogService;
    private readonly IBookingPeriodHelper _bookingPeriodHelper;
    private readonly IFundService _fundService;
    private readonly IErrorManager _errorManager;
    private readonly ILogger<OrderService> _logger;

    /// <summary>
    /// Used for calculating the end balances when processing trades.
    /// </summary>
    record OrderFundingHoldings
    {
        public Guid FundId { get; init; }
        public decimal OrderPercentage { get; init; }
        public Holding BaseHolding { get; init; }
        public Holding QuoteHolding { get; init; }
        public Dictionary<Guid, Holding> FeeHoldings = new();
    }

    public OrderService(
        HodlDbContext dbContext,
        IChangeLogService changeLogService,
        IBookingPeriodHelper bookingPeriodHelper,
        IFundService fundService,
        IErrorManager errorManager,
        ILogger<OrderService> logger)
    {
        _db = dbContext;
        _changeLogService = changeLogService;
        _bookingPeriodHelper = bookingPeriodHelper;
        _fundService = fundService;
        _errorManager = errorManager;
        _logger = logger;
    }

    #region Orders
    public async Task<PagingModel<Order>> GetOrders(
        string orderNumber,
        string cryptoSymbols,
        string walletAddres,
        OrderDirection? direction,
        OrderState? state,
        DateTime? fromDateTime,
        DateTime? toDateTime,
        bool filterFunded,
        int page, int? itemsPerPage,
        CancellationToken cancellationToken = default)
    {
        // When the funded orders must be filtered, create a list if OrderId's
        // to select using an optimized query.
        ICollection<Guid> notFundedCollection = filterFunded
            ? await _db.Orders
                .Where(o => !_db.OrderFundings.Any(f => f.OrderId.Equals(o.Id)))
                .Select(o => o.Id)
                .ToListAsync(cancellationToken)
            : null;

        // First off dealing with the filtering based on given to & from crypto symbols
        var query = string.IsNullOrWhiteSpace(cryptoSymbols)
            ? CreateQueryWithoutCryptoFilter()
            : await CreateQueryUsingCryptoFilter();


        // Create a paged resultset & paged view model instance with the queried data
        return await query.PaginateAsync(page, itemsPerPage ?? DEFAULT_ORDERS_PER_PAGE, cancellationToken);

        /*
         * Local functions
         */
        async Task<IQueryable<Order>> CreateQueryUsingCryptoFilter()
        {
            List<Guid> cc = new();
            foreach (var symbol in cryptoSymbols.Split(splitChars))
            {
                var filterCrypto = new CryptoFilterParams
                {
                    Symbol = symbol
                };

                cc.AddRange(await _db.CryptoCurrencies
                    .AsNoTracking()
                    .Filter(filterCrypto)
                    .Distinct()
                    .OrderBy(c => c.Name)
                    .Select(c => c.Id)
                    .ToArrayAsync(cancellationToken));
            }

            OrderFilterParams filterBaseAsset = new()
            {
                OrderNumber = orderNumber,
                BaseAssetId = cc,
                WalletAddres = walletAddres,
                Direction = direction,
                State = state,
                FromDateTime = fromDateTime,
                ToDateTime = toDateTime,
                Id = notFundedCollection
            };
            OrderFilterParams filterQuoteAsset = new()
            {
                OrderNumber = orderNumber,
                QuoteAssetId = cc,
                WalletAddres = walletAddres,
                Direction = direction,
                State = state,
                FromDateTime = fromDateTime,
                ToDateTime = toDateTime,
                Id = notFundedCollection
            };

            return _db.Orders
                .AsNoTracking()
                .Filter(filterBaseAsset)
                .Union(_db.Orders
                    .AsNoTracking()
                    .Filter(filterQuoteAsset))
                .Distinct()
                .Include(o => o.ExchangeAccount)
                .ThenInclude(a => a.Exchange)
                .Include(o => o.Wallet)
                .Include(o => o.BaseAsset)
                .Include(o => o.QuoteAsset)
                .Include(o => o.OrderFundings.OrderBy(of => of.Fund.FundName))
                .ThenInclude(of => of.Fund)
                .Include(o => o.Trades)
                .ThenInclude(t => t.FeeCurrency)
                .OrderByDescending(o => o.DateTime)
                .AsSplitQuery();
        }

        IQueryable<Order> CreateQueryWithoutCryptoFilter()
        {
            OrderFilterParams filter = new()
            {
                OrderNumber = orderNumber,
                WalletAddres = walletAddres,
                Direction = direction,
                State = state,
                FromDateTime = fromDateTime,
                ToDateTime = toDateTime,
                Id = notFundedCollection
            };

            return _db.Orders
                .AsNoTracking()
                .Filter(filter)
                .Include(o => o.ExchangeAccount)
                .ThenInclude(a => a.Exchange)
                .Include(o => o.Wallet)
                .Include(o => o.BaseAsset)
                .Include(o => o.QuoteAsset)
                .Include(o => o.OrderFundings.OrderBy(of => of.Fund.FundName))
                .ThenInclude(of => of.Fund)
                .Include(o => o.Trades)
                .ThenInclude(t => t.FeeCurrency)
                .OrderByDescending(o => o.DateTime)
                .AsSplitQuery();
        }
    }

    public async Task<Order> GetOrder(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .Where(o => o.Id == orderId)
            .Include(o => o.ExchangeAccount)
            .Include(o => o.Wallet)
            .Include(p => p.BaseAsset)
            .Include(p => p.QuoteAsset)
            .Include(o => o.OrderFundings.OrderBy(of => of.Fund.FundName))
            .ThenInclude(of => of.Fund)
            .Include(o => o.Trades)
            .ThenInclude(t => t.FeeCurrency)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);

        return order;
    }

    public async Task<Order> GetOrderByExchangeOrderId(Guid exchangeAccountId, string exchangeOrderId, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .Where(o => o.ExchangeAccountId == exchangeAccountId && o.ExchangeOrderId == exchangeOrderId)
            .Include(o => o.ExchangeAccount)
            .Include(o => o.Wallet)
            .Include(p => p.BaseAsset)
            .Include(p => p.QuoteAsset)
            .Include(o => o.OrderFundings.OrderBy(of => of.Fund.FundName))
            .ThenInclude(of => of.Fund)
            .Include(o => o.Trades)
            .ThenInclude(t => t.FeeCurrency)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);

        return order;
    }

    public async Task<IEnumerable<Order>> GetOrderByExchangeOrderIdList(Guid exchangeAccountId, string[] exchangeOrderIds, CancellationToken cancellationToken = default)
    {
        var orders = await _db.Orders
            .Where(o => o.ExchangeAccountId == exchangeAccountId && exchangeOrderIds.Contains(o.ExchangeOrderId))
            .ToArrayAsync(cancellationToken);

        return orders;
    }

    public async Task<Trade> GetTradeByExchangeTradeId(Guid exchangeAccountId, string exchangeOrderId, string exchangeTradeId, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .Where(o => o.ExchangeAccountId == exchangeAccountId && o.ExchangeOrderId == exchangeOrderId)
            .SingleOrDefaultAsync(cancellationToken) ??
            throw new NotFoundException($"Order with Exchange order id {exchangeOrderId} nto found.");

        var trade = await _db.Trades
            .Where(t => t.OrderId == order.Id && t.ExchangeTradeId == exchangeTradeId)
            .Include(t => t.Order)
            .Include(t => t.FeeCurrency)
            .AsSingleQuery()
            .SingleOrDefaultAsync(cancellationToken);

        return trade;
    }

    public async Task<Order> InsertOrder(Order order, bool overrideBalanceCheck, bool requireFundings = true, CancellationToken cancellationToken = default)
    {
        if (order.ExchangeAccountId == null && order.WalletAddress == null)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Order.ExchangeAccountId) + "/" + nameof(Order.WalletAddress),
                Code = ErrorCodesStore.InvalidValue,
                Description = "An order must have a reference to an exchange account or a wallet address."
            });

        // Only when manual insert
        if (requireFundings && order.OrderFundings.Count == 0)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(OrderFunding.OrderPercentage),
                Code = ErrorCodesStore.InvalidValue,
                Description = "No fundings were given for the order."
            });

        // Check the currencies, must be active and not locked
        var fromCrypto = await _db.CryptoCurrencies.SingleOrDefaultAsync(c => c.Id == order.QuoteAssetId, cancellationToken);
        if (fromCrypto == null)
        {
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Order.QuoteAsset),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "From crypto currency not found."
            });
        }
        else
        {
            if (!fromCrypto.Active)
                _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
                {
                    Field = nameof(Order.QuoteAsset),
                    Code = ErrorCodesStore.InvalidValue,
                    Description = "From crypto currency is inactive. Orders can only be made on active currencies."
                });
            if (fromCrypto.IsLocked)
                _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
                {
                    Field = nameof(Order.QuoteAsset),
                    Code = ErrorCodesStore.InvalidValue,
                    Description = "From crypto currency is locked. Locked currencies can not be used for trading."
                });
        }

        var toCrypto = await _db.CryptoCurrencies.SingleOrDefaultAsync(c => c.Id == order.BaseAssetId, cancellationToken);
        if (toCrypto == null)
        {
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Order.BaseAsset),
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Description = "To crypto currency not found."
            });
        }
        else
        {
            if (!toCrypto.Active)
                _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
                {
                    Field = nameof(Order.BaseAsset),
                    Code = ErrorCodesStore.InvalidValue,
                    Description = "To crypto currency is inactive. Orders can only be made on active currencies."
                });
            if (toCrypto.IsLocked)
                _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
                {
                    Field = nameof(Order.BaseAsset),
                    Code = ErrorCodesStore.InvalidValue,
                    Description = "To crypto currency is locked. Locked currencies can not be used for trading."
                });
        }

        _errorManager.ThrowOnErrors();

        // Register trading pair
        var _ = await RegisterTradingPair(order.QuoteAssetId, order.BaseAssetId, cancellationToken);

        // Calculate the total, this must be done before the CheckAndFillFundings
        order.Total = order.Amount * order.UnitPrice;
        // Set null instead of empty string because of reference checks in the database
        if (order.WalletAddress == string.Empty) order.WalletAddress = null;

        await CheckAndFillFundings(order, !overrideBalanceCheck, cancellationToken);

        // Save in a transaction
        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Now we can get the order number when it is not given
            if (string.IsNullOrEmpty(order.OrderNumber))
            {
                string navFormat = $"NAV{_bookingPeriodHelper.CalcBookingPeriod(new DateTimeOffset(order.DateTime))}";
                var lastOrderNr = await _db.Orders
                    .Where(o => o.OrderNumber.StartsWith(navFormat))
                    .OrderByDescending(o => o.OrderNumber)
                    .Select(o => o.OrderNumber)
                    .FirstOrDefaultAsync(cancellationToken);

                if (!string.IsNullOrEmpty(lastOrderNr) && int.TryParse(lastOrderNr.Split('-').Last(), out int ordernr))
                {
                    order.OrderNumber = $"{navFormat}-{++ordernr:00}";
                }
                else
                {
                    order.OrderNumber = $"{navFormat}-01";
                }
            }

            await _changeLogService.AddChangeLogAsync("Orders", null, order, cancellationToken);

            await _db.Orders.AddAsync(order, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            transaction.Commit();

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, ex.Message);
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }
    }

    public async Task<Order> UpdateOrder(Guid orderId, Order order, bool requireFundings = true, CancellationToken cancellationToken = default)
    {
        var storedOrder = await _db.Orders
            .Where(f => f.Id == orderId)
            .Include(o => o.OrderFundings)
            .Include(o => o.Trades)
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken) ??
            throw new NotFoundException($"Order with id {orderId} not found");

        // When there are trades on the order, only limited edits can be made.
        // Notify the user about the changes that are not allowed
        if (await _db.Trades.AnyAsync(t => t.OrderId == orderId, cancellationToken) ||
            storedOrder.State != OrderState.New)
        {
            if (order.Direction != storedOrder.Direction)
                _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
                {
                    Field = nameof(Order.Direction),
                    Code = ErrorCodesStore.ValueCannotBeChanged,
                    Description = "The order is already in process so the direction can not be changed anymore."
                });

            if (order.Amount != storedOrder.Amount)
                _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
                {
                    Field = nameof(Order.Amount),
                    Code = ErrorCodesStore.ValueCannotBeChanged,
                    Description = "The order is already in process so the amount can not be changed anymore."
                });
            if (order.UnitPrice != storedOrder.UnitPrice)
                _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
                {
                    Field = nameof(Order.UnitPrice),
                    Code = ErrorCodesStore.ValueCannotBeChanged,
                    Description = "The order is already in process so the unit price can not be changed anymore."
                });
            if (await _db.Trades.AnyAsync(t => t.OrderId == orderId && !string.IsNullOrWhiteSpace(t.BookingPeriod), cancellationToken))
            {
                _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
                {
                    Field = nameof(Trade.BookingPeriod),
                    Code = ErrorCodesStore.ValueCannotBeChanged,
                    Description = "The order has registered trades in a closed bookingperiod so can not be changed anymore."
                });
            }
        }

        if (requireFundings && order.OrderFundings.Count == 0)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(OrderFunding.OrderPercentage),
                Code = ErrorCodesStore.InvalidValue,
                Description = "No fundings were given for the order."
            });

        _errorManager.ThrowOnErrors();

        // Save in a transaction
        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _changeLogService.AddChangeLogAsync("Orders", storedOrder, order, cancellationToken);

            // Revert the trade values on the holdings
            foreach (var trade in storedOrder.Trades)
            {
                foreach (var orderFunding in storedOrder.OrderFundings)
                {
                    if (orderFunding.OrderPercentage == 0)
                        continue;

                    var holdingFrom = await _fundService.GetOrCreateFundHolding(orderFunding.FundId, storedOrder.QuoteAssetId, cancellationToken: cancellationToken);
                    var holdingTo = await _fundService.GetOrCreateFundHolding(orderFunding.FundId, storedOrder.BaseAssetId, cancellationToken: cancellationToken);

                    switch (order.Direction)
                    {
                        case OrderDirection.Buy:
                            holdingFrom.EndBalance += (trade.Total * orderFunding.OrderPercentage / 100);
                            holdingTo.EndBalance -= (trade.Executed * orderFunding.OrderPercentage / 100);
                            break;
                        case OrderDirection.Sell:
                            holdingFrom.EndBalance -= (trade.Total * orderFunding.OrderPercentage / 100);
                            holdingTo.EndBalance += (trade.Executed * orderFunding.OrderPercentage / 100);
                            break;
                        default: // Default buy
                            holdingFrom.EndBalance += (trade.Total * orderFunding.OrderPercentage / 100);
                            holdingTo.EndBalance -= (trade.Executed * orderFunding.OrderPercentage / 100);
                            break;
                    }

                    var holdingFee = trade.FeeCurrencyId == storedOrder.QuoteAssetId
                        ? holdingFrom
                        : trade.FeeCurrencyId == storedOrder.BaseAssetId
                        ? holdingTo
                        : await _fundService.GetOrCreateFundHolding(orderFunding.FundId, trade.FeeCurrencyId, cancellationToken: cancellationToken);
                    holdingFee.EndBalance += (trade.Fee * orderFunding.OrderPercentage / 100);
                }
            }

            // TODO: Take over the new values
            storedOrder.OrderNumber = order.OrderNumber;
            storedOrder.DateTime = order.DateTime;
            storedOrder.Type = order.Type;
            storedOrder.Direction = order.Direction;
            storedOrder.State = order.State;
            storedOrder.UnitPrice = order.UnitPrice;
            storedOrder.Amount = order.Amount;
            storedOrder.Total = order.Amount * order.UnitPrice;
            foreach (var funding in order.OrderFundings)
            {
                var orderFunding = storedOrder.OrderFundings.FirstOrDefault(f => f.FundId == funding.FundId);
                if (orderFunding != null)
                {
                    orderFunding.OrderAmount = funding.OrderAmount;
                    orderFunding.OrderPercentage = funding.OrderPercentage;
                }
                else
                {
                    storedOrder.OrderFundings.Add(new OrderFunding()
                    {
                        OrderId = storedOrder.Id,
                        FundId = funding.FundId,
                        OrderAmount = funding.OrderAmount,
                        OrderPercentage = funding.OrderPercentage
                    });
                }
            }

            await CheckAndFillFundings(storedOrder, false, cancellationToken);

            // Redo the trades that are made
            foreach (var trade in storedOrder.Trades)
            {
                foreach (var orderFunding in storedOrder.OrderFundings)
                {
                    if (orderFunding.OrderPercentage == 0)
                        continue;

                    var holdingFrom = await _fundService.GetOrCreateFundHolding(orderFunding.FundId, storedOrder.QuoteAssetId, cancellationToken: cancellationToken);
                    var holdingTo = await _fundService.GetOrCreateFundHolding(orderFunding.FundId, storedOrder.BaseAssetId, cancellationToken: cancellationToken);

                    switch (order.Direction)
                    {
                        case OrderDirection.Buy:
                            holdingFrom.EndBalance -= (trade.Total * orderFunding.OrderPercentage / 100);
                            holdingTo.EndBalance += (trade.Executed * orderFunding.OrderPercentage / 100);
                            break;
                        case OrderDirection.Sell:
                            holdingFrom.EndBalance += (trade.Total * orderFunding.OrderPercentage / 100);
                            holdingTo.EndBalance -= (trade.Executed * orderFunding.OrderPercentage / 100);
                            break;
                        default: // Default buy
                            holdingFrom.EndBalance -= (trade.Total * orderFunding.OrderPercentage / 100);
                            holdingTo.EndBalance += (trade.Executed * orderFunding.OrderPercentage / 100);
                            break;
                    }

                    var holdingFee = trade.FeeCurrencyId == storedOrder.QuoteAssetId
                        ? holdingFrom
                        : trade.FeeCurrencyId == storedOrder.BaseAssetId
                        ? holdingTo
                        : await _fundService.GetOrCreateFundHolding(orderFunding.FundId, trade.FeeCurrencyId, cancellationToken: cancellationToken);
                    holdingFee.EndBalance -= (trade.Fee * orderFunding.OrderPercentage / 100);
                }
            }
            await _db.SaveChangesAsync(cancellationToken);

            await RecalcOrderState(orderId, cancellationToken);


            transaction.Commit();

            return storedOrder;
        }
        catch (Exception ex)
        {
            _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, ex.Message);
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }
    }

    public async Task<bool> DeleteOrder(Guid orderId, CancellationToken cancellationToken = default)
    {
        var storedOrder = await _db.Orders
            .Where(c => c.Id == orderId)
            .SingleOrDefaultAsync(cancellationToken) ??
            throw new NotFoundException($"Order with id {orderId} not found");

        // Check the state and related trades. If any trades are made the order can not be removed.
        if (await _db.Trades.AnyAsync(t => t.OrderId == orderId, cancellationToken))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Code = ErrorCodesStore.ItemIsReferenced,
                Description = "There are trades registered on this order so it can not be removed."
            });

        _errorManager.ThrowOnErrors();

        // Save in a transaction
        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _changeLogService.AddChangeLogAsync("Orders", storedOrder, null, cancellationToken);
            _db.Orders.Remove(storedOrder);
            await _db.SaveChangesAsync(cancellationToken);

            transaction.Commit();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, ex.Message);
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }
    }

    private async Task CheckAndFillFundings(Order order, bool checkBalance, CancellationToken cancellationToken = default)
    {
        if (order.OrderFundings == null || order.OrderFundings.Count == 0)
            return;

        foreach (var orderFunding in order.OrderFundings)
        {
            orderFunding.OrderId = order.Id;

            if (orderFunding.OrderPercentage > 0)
            {
                // Calculate the values from the percentages of the order amount
                orderFunding.OrderAmount = order.Amount * (orderFunding.OrderPercentage / 100);
            }
            else
            {
                // Calculate the percentages from the fixed ammounts
                orderFunding.OrderPercentage = orderFunding.OrderAmount / order.Amount * 100;
            }
            orderFunding.OrderTotal = order.Total * (orderFunding.OrderPercentage / 100);
        }

        // Check if the complete order is funded (sum of orderfundings percentages must be exactly 100%)
        var totalPercentage = order.OrderFundings.Aggregate(0m, (sum, of) => sum += of.OrderPercentage);
        var totalAmmount = order.OrderFundings.Aggregate(0m, (sum, of) => sum += of.OrderAmount);

        if ((totalPercentage != 0 && totalPercentage != 100) ||
            (totalAmmount > 0 && totalAmmount != order.Amount))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(OrderFunding.OrderPercentage),
                Code = ErrorCodesStore.InvalidValue,
                Description = "The sum of the order fundings must be exactly 100% and the same amount as the order amount."
            });

        _errorManager.ThrowOnErrors();

        // Check if all fundings have (enough) holdings for the selected currency
        foreach (var orderFunding in order.OrderFundings)
        {
            var currenBookingPeriod = await _fundService.CurrentBookingPeriod(orderFunding.FundId, cancellationToken);

            // Get the fund holding to check the funding of the order
            var checkCryptoId = order.Direction switch
            {
                OrderDirection.Buy => order.QuoteAssetId,
                OrderDirection.Sell => order.BaseAssetId,
                _ => order.QuoteAssetId
            };
            var checkHoldingAmount = order.Direction switch
            {
                OrderDirection.Buy => orderFunding.OrderTotal,
                OrderDirection.Sell => orderFunding.OrderAmount,
                _ => order.Total
            };

            // No checks needed when no ammount is to be booked
            if (checkHoldingAmount == 0) continue;

            var holding = await _db.Holdings
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.FundId == orderFunding.FundId
                    && h.CryptoId == checkCryptoId
                    && h.BookingPeriod == currenBookingPeriod,
                    cancellationToken);

            if (holding == null)
            {
                _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
                {
                    Field = nameof(Order.QuoteAssetId),
                    Code = ErrorCodesStore.InvalidValue,
                    Description = "One of the funds has no holdings for the currency on the order."
                });
            }
            if (checkBalance && holding != null && holding.EndBalance < checkHoldingAmount)
            {
                _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
                {
                    Field = nameof(OrderFunding.OrderAmount),
                    Code = ErrorCodesStore.InvalidValue,
                    Description = "One of the funds has not enough balance to fulfill the order amount."
                });
            }

            _errorManager.ThrowOnErrors();
        }
    }

    #endregion

    #region Trades

    public async Task<PagingModel<Trade>> GetTrades(Guid orderId, int page, int? itemsPerPage, CancellationToken cancellationToken = default)
    {
        var query = _db.Trades
            .AsNoTracking()
            .Where(t => t.OrderId == orderId)
            .Include(t => t.FeeCurrency)
            .AsSingleQuery()
            .OrderByDescending(t => t.DateTime);

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? DEFAULT_TRADES_PER_PAGE, cancellationToken);

        return pageResult;
    }

    public async Task<Trade> GetTrade(Guid orderId, Guid tradeId, CancellationToken cancellationToken = default)
    {
        var trade = await _db.Trades
            .AsNoTracking()
            .Where(t => t.OrderId == orderId && t.Id == tradeId)
            .Include(t => t.Order)
            .Include(t => t.FeeCurrency)
            .AsSingleQuery()
            .FirstOrDefaultAsync(cancellationToken);

        return trade;
    }

    public async Task<Trade> InsertTrade(Guid orderId, Trade trade, bool checkOrderState = true, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .Include(o => o.OrderFundings)
            .Include(o => o.Trades)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken) ??
            throw new NotFoundException($"Order with id {orderId} not found");

        if (!await _db.CryptoCurrencies.AnyAsync(c => c.Id == trade.FeeCurrencyId, cancellationToken))
            _errorManager.AddValidationError(HttpStatusCode.FailedDependency, new ErrorInformationItem
            {
                Code = ErrorCodesStore.ReferencedRecordNotFound,
                Field = nameof(Trade.FeeCurrency),
                Description = "Fee currency not found."
            });

        if (checkOrderState && order.State == OrderState.Filled)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Code = ErrorCodesStore.OrderStateError,
                Description = "Can not add trades on filled order."
            });

        if (checkOrderState && order.State == OrderState.Cancelled)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Code = ErrorCodesStore.OrderStateError,
                Description = "Can not add trades on cancelede order."
            });

        if (order.Trades.Sum(t => t.Executed) + trade.Executed > order.Amount)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Code = ErrorCodesStore.ValueExceedsOrder,
                Description = "Trade execution value exceeds the order amount."
            });

        _errorManager.ThrowOnErrors();

        trade.OrderId = order.Id;
        trade.Total = trade.Executed * trade.UnitPrice;

        // Save in a transaction
        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _changeLogService.AddChangeLogAsync("Trades", null, trade, cancellationToken);
            await _db.Trades.AddAsync(trade, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            await RecalcOrderState(order.Id, cancellationToken);

            // Add the values on the holdings
            foreach (var orderFunding in order.OrderFundings)
            {
                if (orderFunding.OrderPercentage == 0)
                    continue;

                var holdingFrom = await _fundService.GetOrCreateFundHolding(orderFunding.FundId, order.QuoteAssetId, cancellationToken: cancellationToken);
                var holdingTo = await _fundService.GetOrCreateFundHolding(orderFunding.FundId, order.BaseAssetId, cancellationToken: cancellationToken);

                var bookingPeriod = _bookingPeriodHelper.CalcBookingPeriod(new DateTimeOffset(trade.DateTime));

                // Only modify the balances if the booking period for the trade
                // datetime is the same as the holding and the booking period
                // is not closed. When an old trade is added after the booking
                // period is closed, the period should be recalculated.
                if (!holdingFrom.BookingPeriod.Equals(bookingPeriod) || holdingFrom.PeriodClosedDateTime != null ||
                    !holdingTo.BookingPeriod.Equals(bookingPeriod) || holdingTo.PeriodClosedDateTime != null)
                    continue;

                switch (order.Direction)
                {
                    case OrderDirection.Buy:
                        holdingFrom.EndBalance -= (trade.Total * orderFunding.OrderPercentage / 100);
                        holdingTo.EndBalance += (trade.Executed * orderFunding.OrderPercentage / 100);
                        break;
                    case OrderDirection.Sell:
                        holdingFrom.EndBalance += (trade.Total * orderFunding.OrderPercentage / 100);
                        holdingTo.EndBalance -= (trade.Executed * orderFunding.OrderPercentage / 100);
                        break;
                    default: // Default buy
                        holdingFrom.EndBalance -= (trade.Total * orderFunding.OrderPercentage / 100);
                        holdingTo.EndBalance += (trade.Executed * orderFunding.OrderPercentage / 100);
                        break;
                }

                var holdingFee = trade.FeeCurrencyId == order.QuoteAssetId
                    ? holdingFrom
                    : trade.FeeCurrencyId == order.BaseAssetId
                    ? holdingTo
                    : await _fundService.GetOrCreateFundHolding(orderFunding.FundId, trade.FeeCurrencyId, cancellationToken: cancellationToken);
                holdingFee.EndBalance -= (trade.Fee * orderFunding.OrderPercentage / 100);

                await _db.SaveChangesAsync(cancellationToken);
            }

            transaction.Commit();

            return trade;
        }
        catch (Exception ex)
        {
            _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, ex.Message);
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }
    }

    public async Task<IEnumerable<Trade>> InsertTrades(Guid orderId, IEnumerable<Trade> trades, bool checkOrderState = true, CancellationToken cancellationToken = default)
    {
        if (trades == null || !trades.Any()) return null;

        var order = await _db.Orders
            .Include(o => o.OrderFundings)
            .Include(o => o.Trades)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken) ??
            throw new NotFoundException($"Order with id {orderId} not found");

        if (checkOrderState && order.State == OrderState.Filled)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Code = ErrorCodesStore.OrderStateError,
                Description = "Can not add trades on filled order."
            });

        if (checkOrderState && order.State == OrderState.Cancelled)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Code = ErrorCodesStore.OrderStateError,
                Description = "Can not add trades on cancelede order."
            });

        if (order.Trades.Sum(t => t.Executed) + trades.Sum(t => t.Executed) > order.Amount)
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Code = ErrorCodesStore.ValueExceedsOrder,
                Description = "Trade execution value exceeds the order amount."
            });

        _errorManager.ThrowOnErrors();

        // Save in a transaction
        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // The add result set
            List<Trade> newTrades = new();

            // Get the holdings to increase/decrease the end balances
            var bookingPeriod = _bookingPeriodHelper.CalcBookingPeriod(new DateTimeOffset(trades.First().DateTime));
            List<OrderFundingHoldings> fundings = new();
            foreach (var orderFunding in order.OrderFundings)
            {
                if (orderFunding.OrderPercentage == 0)
                    continue;

                fundings.Add(new OrderFundingHoldings()
                {
                    FundId = orderFunding.FundId,
                    OrderPercentage = orderFunding.OrderPercentage,
                    BaseHolding = await _fundService.GetOrCreateFundHolding(orderFunding.FundId, order.BaseAssetId, cancellationToken: cancellationToken),
                    QuoteHolding = await _fundService.GetOrCreateFundHolding(orderFunding.FundId, order.QuoteAssetId, cancellationToken: cancellationToken)
                });
            }


            // Start the loop for checking and calculations of the funding balances
            foreach (Trade trade in trades)
            {
                trade.OrderId = order.Id;
                trade.Total = trade.Executed * trade.UnitPrice;

                await _changeLogService.AddChangeLogAsync("Trades", null, trade, cancellationToken);
                newTrades.Add(trade);

                foreach (var funding in fundings)
                {
                    // Only modify the balances if the booking period for the trade
                    // datetime is the same as the holding and the booking period
                    // is not closed. When an old trade is added after the booking
                    // period is closed, the period should be recalculated.
                    if (!funding.QuoteHolding.BookingPeriod.Equals(bookingPeriod) || funding.QuoteHolding.PeriodClosedDateTime != null ||
                        !funding.BaseHolding.BookingPeriod.Equals(bookingPeriod) || funding.BaseHolding.PeriodClosedDateTime != null)
                        continue;

                    // And calculate the fundings when available
                    switch (order.Direction)
                    {
                        case OrderDirection.Buy:
                            funding.QuoteHolding.EndBalance -= (trade.Total * funding.OrderPercentage / 100);
                            funding.BaseHolding.EndBalance += (trade.Executed * funding.OrderPercentage / 100);
                            break;
                        case OrderDirection.Sell:
                            funding.QuoteHolding.EndBalance += (trade.Total * funding.OrderPercentage / 100);
                            funding.BaseHolding.EndBalance -= (trade.Executed * funding.OrderPercentage / 100);
                            break;
                        default: // Default buy
                            funding.QuoteHolding.EndBalance -= (trade.Total * funding.OrderPercentage / 100);
                            funding.BaseHolding.EndBalance += (trade.Executed * funding.OrderPercentage / 100);
                            break;
                    }

                    if (!funding.FeeHoldings.ContainsKey(trade.FeeCurrencyId))
                        funding.FeeHoldings[trade.FeeCurrencyId] = trade.FeeCurrencyId.Equals(funding.QuoteHolding)
                            ? funding.QuoteHolding
                            : trade.FeeCurrencyId.Equals(funding.BaseHolding)
                            ? funding.BaseHolding
                            : await _fundService.GetOrCreateFundHolding(funding.FundId, trade.FeeCurrencyId, cancellationToken: cancellationToken);

                    funding.FeeHoldings[trade.FeeCurrencyId].EndBalance -= (trade.Fee * funding.OrderPercentage / 100);
                }
            }

            await _db.Trades.AddRangeAsync(newTrades, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            await RecalcOrderState(order.Id, cancellationToken);

            transaction.Commit();

            return newTrades;
        }
        catch (Exception ex)
        {
            _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, ex.Message);
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }
    }

    public async Task<bool> DeleteTrade(Guid orderId, Guid tradeId, CancellationToken cancellationToken = default)
    {
        var storedTrade = await _db.Trades
            .Where(t => t.OrderId == orderId && t.Id == tradeId)
            .Include(t => t.Order)
            .ThenInclude(o => o.OrderFundings)
            .AsSingleQuery()
            .FirstOrDefaultAsync(cancellationToken) ??
            throw new NotFoundException($"Trade with id {tradeId} not found");

        if (!string.IsNullOrWhiteSpace(storedTrade.BookingPeriod))
            _errorManager.AddValidationError(HttpStatusCode.Conflict, new ErrorInformationItem
            {
                Field = nameof(Trade.BookingPeriod),
                Code = ErrorCodesStore.BookingPeriodClosed,
                Description = "The trade is recorded in a closed bookingperiod so can not be removed."
            });

        _errorManager.ThrowOnErrors();

        var order = await _db.Orders
            .Include(o => o.OrderFundings)
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        // Save in a transaction
        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _changeLogService.AddChangeLogAsync("Trades", storedTrade, null, cancellationToken);

            // Revert the values on the holdings
            foreach (var orderFunding in order.OrderFundings)
            {
                if (orderFunding.OrderPercentage == 0)
                    continue;

                var holdingFrom = await _fundService.GetOrCreateFundHolding(orderFunding.FundId, order.QuoteAssetId, cancellationToken: cancellationToken);
                var holdingTo = await _fundService.GetOrCreateFundHolding(orderFunding.FundId, order.BaseAssetId, cancellationToken: cancellationToken);

                switch (order.Direction)
                {
                    case OrderDirection.Buy:
                        holdingFrom.EndBalance += (storedTrade.Total * orderFunding.OrderPercentage / 100);
                        holdingTo.EndBalance -= (storedTrade.Executed * orderFunding.OrderPercentage / 100);
                        break;
                    case OrderDirection.Sell:
                        holdingFrom.EndBalance -= (storedTrade.Total * orderFunding.OrderPercentage / 100);
                        holdingTo.EndBalance += (storedTrade.Executed * orderFunding.OrderPercentage / 100);
                        break;
                    default: // Default buy
                        holdingFrom.EndBalance += (storedTrade.Total * orderFunding.OrderPercentage / 100);
                        holdingTo.EndBalance -= (storedTrade.Executed * orderFunding.OrderPercentage / 100);
                        break;
                }

                var holdingFee = storedTrade.FeeCurrencyId == order.QuoteAssetId
                    ? holdingFrom
                    : storedTrade.FeeCurrencyId == order.BaseAssetId
                    ? holdingTo
                    : await _fundService.GetOrCreateFundHolding(orderFunding.FundId, storedTrade.FeeCurrencyId, cancellationToken: cancellationToken);
                holdingFee.EndBalance += (storedTrade.Fee * orderFunding.OrderPercentage / 100);

                await _db.SaveChangesAsync(cancellationToken);
            }

            _db.Trades.Remove(storedTrade);
            await _db.SaveChangesAsync(cancellationToken);

            await RecalcOrderState(orderId, cancellationToken);

            transaction.Commit();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, ex.Message);
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }
    }

    /// <summary>
    /// Calculates the sum of the filled amount of the order and sets the state.
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    private async Task RecalcOrderState(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .Where(o => o.Id == orderId)
            .Include(o => o.Trades)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

        var executed = order.Trades.Sum(t => t.Executed);

        order.State = executed == 0
            ? OrderState.New
            : executed < order.Amount
            ? OrderState.PartFilled
            : OrderState.Filled;

        order.Total = executed >= order.Amount
            ? order.Trades.Sum(t => t.Total)
            : order.Trades.Sum(t => t.Total) + ((order.Amount - executed) * order.UnitPrice);

        await _db.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region TradePairs

    public async Task<PagingModel<Pair>> GetPairs(
        Guid cryptoId,
        int page, int? itemsPerPage,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Pairs
            .AsNoTracking()
            .Where(p => p.QuoteAssetId == cryptoId || p.BaseAssetId == cryptoId)
            .Include(p => p.BaseAsset)
            .Include(p => p.QuoteAsset)
            .OrderBy(p => p.PairString)
            .AsSingleQuery();

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, cancellationToken);

        return pageResult;
    }

    public async Task<PagingModel<Pair>> FindPairs(
        string pairstring,
        int page, int? itemsPerPage,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Pairs
            .AsNoTracking()
            .Where(p => p.PairString.ToUpper().Contains(pairstring.ToUpper()))
            .Include(p => p.BaseAsset)
            .Include(p => p.QuoteAsset)
            .OrderBy(p => p.PairString)
            .AsSingleQuery();

        // Create a paged resultset
        var pageResult = await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, cancellationToken);

        return pageResult;
    }

    public async Task<bool> DeletePair(string pairstring, CancellationToken cancellationToken = default)
    {
        var storedPair = await _db.Pairs
            .Where(p => p.PairString == HttpUtility.UrlDecode(pairstring))
            .FirstOrDefaultAsync(cancellationToken) ??
            throw new NotFoundException($"Trading pair {pairstring} not found");

        using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _changeLogService.AddChangeLogAsync("Pairs", storedPair, null, cancellationToken);
            _db.Pairs.Remove(storedPair);
            await _db.SaveChangesAsync(cancellationToken);

            transaction.Commit();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, ex.Message);
            transaction.Rollback();
            throw new RestException(HttpStatusCode.InternalServerError, new ErrorInformationItem
            {
                Code = ErrorCodesStore.InternalServerError,
                Description = ex.Message
            });
        }
    }

    private async Task<string> PairString(Guid quoteAssetId, Guid baseAssetId, CancellationToken cancellationToken = default)
    {
        string quoteSymbol = await _db.CryptoCurrencies
            .Where(c => c.Id == quoteAssetId)
            .Select(c => c.Symbol)
            .SingleOrDefaultAsync(cancellationToken);

        string baseSymbol = await _db.CryptoCurrencies
            .Where(c => c.Id == baseAssetId)
            .Select(c => c.Symbol)
            .SingleOrDefaultAsync(cancellationToken);

        return $"{baseSymbol}/{quoteSymbol}";
    }

    private async Task<Pair> RegisterTradingPair(Guid quoteAssetId, Guid baseAssetId, CancellationToken cancellationToken = default)
    {
        Pair result = await _db.Pairs
            .FirstOrDefaultAsync(p => p.QuoteAssetId == quoteAssetId && p.BaseAssetId == baseAssetId, cancellationToken);

        if (result == null)
        {
            // Add the pair to the pair table
            result = new Pair()
            {
                PairString = await PairString(quoteAssetId, baseAssetId, cancellationToken),
                BaseAssetId = baseAssetId,
                QuoteAssetId = quoteAssetId
            };

            _db.Pairs.Add(result);
            _db.SaveChanges();
        }

        return result;
    }

    #endregion
}
