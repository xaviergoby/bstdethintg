using CryptoExchange.Net.Sockets;
using Hodl.ExchangeAPI.Constants;
using Hodl.ExchangeAPI.Models;
using Kucoin.Net.Clients;
using Kucoin.Net.Enums;
using Kucoin.Net.Objects;
using Kucoin.Net.Objects.Models.Spot;
using Kucoin.Net.Objects.Models.Spot.Socket;
using Microsoft.Extensions.Logging;

namespace Hodl.ExchangeAPI.Kucoin;
public class ExchangeKucoin : BaseExchangeAPI
{
    // Test Kucoin
    // API Key: 63c16d512b968a00015429c8
    // Secret: 68ada3dd-a47e-4bbf-8fd6-a9932e3cd987
    // Passphrase: ZvaoGTiUg7*9oJ24y#8b2qj^%u

    private readonly KucoinClient _kucoinClient;
    private readonly KucoinSocketClient _kucoinSocketClient;

    private UpdateSubscription updateSubscription;

    public ExchangeKucoin(Guid accountId, string key, string secret, string passPhrase, bool testEnvironment) : base(accountId, key)
    {
        var credentials = new KucoinApiCredentials(key, secret, passPhrase);
        _kucoinClient = new KucoinClient(new KucoinClientOptions
        {
            ApiCredentials = credentials,
            SpotApiOptions = new KucoinRestApiClientOptions()
            {
                BaseAddress = testEnvironment
                    ? "https://openapi-sandbox.kucoin.com/api"
                    //: "https://api.kucoin.com/api",
                    : "https://openapi-v2.kucoin.com/api",
            },
            LogLevel = LogLevel.Debug
        });

        _kucoinSocketClient = new KucoinSocketClient(new KucoinSocketClientOptions
        {
            ApiCredentials = credentials,
            SpotStreamsOptions = new KucoinSocketApiClientOptions
            {
                BaseAddress = testEnvironment
                    ? "wss://openapi-sandbox.kucoin.com"
                    : "wss://push1-v2.kucoin.com",
                AutoReconnect = true,
                ApiCredentials = credentials,
            },
            LogLevel = LogLevel.Debug
        });
    }

    #region Dispose
    protected override void Dispose(bool disposing)
    {
        if (!base._disposedValue)
        {
            if (disposing)
            {
                if (updateSubscription != null)
                    Task.Run(async () => await _kucoinSocketClient.UnsubscribeAsync(updateSubscription)).Wait();

                // Disconnect the API
                _kucoinSocketClient.Dispose();
                _kucoinClient.Dispose();
            }

            base._disposedValue = true;
        }
    }
    #endregion

    public override async Task<IEnumerable<ExchangeOrder>> GetOrders(string symbol, DateTime startTime, CancellationToken cancelationToken = default)
    {
        if (string.IsNullOrEmpty(symbol))
            throw new ArgumentNullException(nameof(symbol));

        // ExchangeAccoutnId: 75a57aff-0e38-45db-bc1e-052b3dc32044
        var result = new List<ExchangeOrder>();

        string kucoinSymbol = symbol.Replace("/", "-");

        // Get the open orders
        var webResult1 = await _kucoinClient.SpotApi.Trading.GetOrdersAsync(
            symbol: kucoinSymbol,
            //tradeType: TradeType.SpotTrade, 
            status: OrderStatus.Active,
            //startTime: startTime, 
            currentPage: 1,
            pageSize: 100,
            ct: cancelationToken);

        if (webResult1.Success)
        {
            result.AddRange(webResult1.Data.Items
                .Select(o => OrderFromKucoinOrder(o))
                .ToList());
        }

        // Get the Done orders
        var webResult2 = await _kucoinClient.SpotApi.Trading.GetOrdersAsync(
            symbol: kucoinSymbol,
            //tradeType: TradeType.SpotTrade, 
            status: OrderStatus.Done,
            //startTime: startTime, 
            currentPage: 1,
            pageSize: 100,
            ct: cancelationToken);

        if (webResult2.Success)
        {
            result.AddRange(webResult2.Data.Items
                .Select(o => OrderFromKucoinOrder(o))
                .ToList());
        }

        return result;
    }

    public override async Task<ExchangeOrder> GetOrder(string symbol, string orderId, CancellationToken cancelationToken = default)
    {
        var webResult = await _kucoinClient.SpotApi.Trading.GetOrderAsync(orderId: orderId, ct: cancelationToken);

        if (webResult.Success)
        {
            return OrderFromKucoinOrder(webResult.Data);
        }

        throw new KeyNotFoundException($"Exchange order {orderId} on Kucoin not found");
    }

    public override async Task<IEnumerable<ExchangeTrade>> GetOrderTrades(string symbol, string orderId, CancellationToken cancelationToken = default)
    {
        var webResult = await _kucoinClient.SpotApi.Trading.GetUserTradesAsync(orderId: orderId, ct: cancelationToken);

        if (webResult.Success)
        {
            return webResult.Data.Items
                .Select(t => TradeFromKucoinTrade(t))
                .ToList();
        }

        return new List<ExchangeTrade>();
    }

    public override async Task<bool> SubscribeOrderUpdates(CancellationToken cancelationToken = default)
    {
        // Add event handlers for orders and trades
        var sub = await _kucoinSocketClient.SpotStreams.SubscribeToOrderUpdatesAsync(
            OnOrderUpdateMessage,
            OnTradeUpdateMessage,
            cancelationToken);

        if (sub.Success)
            updateSubscription = sub.Data;

        return sub.Success;
    }

    public override async Task<IEnumerable<ExchangeBalance>> GetBalances(CancellationToken cancelationToken = default)
    {
        var webResult = await _kucoinClient.SpotApi.Account.GetAccountsAsync(ct: cancelationToken);

        if (webResult.Success)
        {
            return webResult.Data
                .Select(a => BalanceFromKucoinAccount(a))
                .ToList();
        }

        return new List<ExchangeBalance>();
    }

    private void OnOrderUpdateMessage(DataEvent<KucoinStreamOrderBaseUpdate> data)
    {
        // Handle order update
        try
        {
            switch (data.Data.UpdateType)
            {
                case MatchUpdateType.Open:
                    base.OnNewOrder(OrderFromData(data.Data));
                    break;
                default:
                    base.OnUpdateOrder(OrderFromData(data.Data));
                    break;
            }
        }
        catch (Exception e)
        {
            // We want to log the error!!
            Console.WriteLine($"Binance order update event reported an error: {e}");
        }
    }

    private void OnTradeUpdateMessage(DataEvent<KucoinStreamOrderMatchUpdate> data)
    {
        base.OnNewTrade(TradeFromData(data.Data));
    }

    private ExchangeOrder OrderFromKucoinOrder(KucoinOrder order) => new()
    {
        OrderId = order.Id,
        ClientOrderId = order.ClientOrderId,
        DateTime = order.CreateTime,
        BaseAsset = GetBaseAsset(order.Symbol),
        QuoteAsset = GetQuoteAsset(order.Symbol),
        Type = order.Type.ToString(),
        Direction = ConvertOrderSide(order.Side),
        //State = ConvertOrderStatus(order.??),
        UnitPrice = order.Price == null ? 0 : (decimal)order.Price,
        Amount = order.Quantity == null ? 0 : (decimal)order.Quantity,
        Total = order.QuoteQuantity == null ? 0 : (decimal)order.QuoteQuantity,
        IsMaker = false,
        IsTaker = true
    };

    private ExchangeOrder OrderFromData(KucoinStreamOrderBaseUpdate order) => new()
    {
        OrderId = order.OrderId.ToString(),
        ClientOrderId = order.ClientOrderid,
        DateTime = order.Timestamp,
        BaseAsset = GetBaseAsset(order.Symbol),
        QuoteAsset = GetQuoteAsset(order.Symbol),
        Type = order.OrderType.ToString(),
        Direction = ConvertOrderSide(order.Side),
        State = ConvertOrderStatus(order.UpdateType),
        UnitPrice = order.Price,
        Amount = order.Quantity,
        Total = order.Quantity * order.Price
    };

    private static ExchangeTrade TradeFromKucoinTrade(KucoinUserTrade trade) => new()
    {
        Id = trade.Id,
        OrderId = trade.OrderId,
        TransactionId = "-",
        DateTime = trade.Timestamp,
        UnitPrice = trade.Price,
        Executed = trade.Quantity,
        Total = trade.QuoteQuantity,
        Fee = trade.Fee,
        FeeAsset = GetFeeAsset(trade.FeeAsset)
    };

    private ExchangeTrade TradeFromData(KucoinStreamOrderMatchUpdate match) => new()
    {
        Id = match.TradeId,
        OrderId = match.OrderId,
        TransactionId = "-",
        DateTime = match.Timestamp,
        UnitPrice = match.MatchPrice,
        Executed = match.MatchQuantity,
        Total = match.MatchQuantity * match.MatchPrice,
        Fee = 0, // TODO: Find out where the fee is represented,
        FeeAsset = GetQuoteAsset(match.Symbol)
    };

    private static ExchangeBalance BalanceFromKucoinAccount(KucoinAccount account) => new()
    {
        Asset = account.Asset,
        Name = account.Asset,
        Locked = account.Holds,
        Available = account.Available,
        Total = account.Total
    };

    private async Task<KucoinSymbol> GetSpotSymbol(string symbol)
    {
        if (await UpdateMarketData())
        {
            var kucoinSymbol = spotInfo?.FirstOrDefault(s => s.Name.Equals(symbol));

            if (kucoinSymbol == null && await UpdateMarketData(forceUpdate: true))
            {
                kucoinSymbol = spotInfo?.FirstOrDefault(s => s.Name.Equals(symbol));
            }

            if (kucoinSymbol != null)
                return kucoinSymbol;
        }

        throw new Exception($"Kucoin symbol not found: {symbol}");
    }

    private ExchangeCryptoCurrency GetBaseAsset(string symbol)
    {
        var kucoinSymbol = Task.Run(async () => await GetSpotSymbol(symbol)).Result;

        return new ExchangeCryptoCurrency
        {
            Name = kucoinSymbol.BaseAsset,
            Symbol = kucoinSymbol.BaseAsset,
        };
    }

    private ExchangeCryptoCurrency GetQuoteAsset(string symbol)
    {
        var kucoinSymbol = Task.Run(async () => await GetSpotSymbol(symbol)).Result;

        return new ExchangeCryptoCurrency
        {
            Name = kucoinSymbol.QuoteAsset,
            Symbol = kucoinSymbol.QuoteAsset
        };
    }

    private static ExchangeCryptoCurrency GetFeeAsset(string assetSymbol)
    {
        return new ExchangeCryptoCurrency
        {
            Name = assetSymbol,
            Symbol = assetSymbol,
        };
    }

    private static readonly object spotLockObject = new();
    private static object spotUpdateObject = null;
    private static DateTime spotInfoLastUpdate = DateTime.MinValue;
    private static IEnumerable<KucoinSymbol> spotInfo;

    private async Task<bool> UpdateMarketData(bool forceUpdate = false, CancellationToken cancellationToken = default)
    {
        if ((forceUpdate || DateTime.UtcNow.Subtract(spotInfoLastUpdate).TotalHours >= 24) && spotUpdateObject == null)
        {
            lock (spotLockObject)
            {
                // Register as updating object since multiple instances can eb created
                spotUpdateObject = this;
            }
            try
            {
                // Get the Market info
                var webCallResult = await _kucoinClient.SpotApi.ExchangeData.GetSymbolsAsync(ct: cancellationToken);

                lock (spotLockObject)
                {
                    if (webCallResult.Success)
                    {
                        spotInfo = webCallResult.Data;
                        spotInfoLastUpdate = DateTime.UtcNow;
                    }
                }
            }
            finally
            {
                if (spotUpdateObject == this)
                    lock (spotLockObject)
                    {
                        spotUpdateObject = null;
                    }
            }
        }

        return spotInfo != null;
    }

    private static OrderDirection ConvertOrderSide(OrderSide side) => side switch
    {
        OrderSide.Buy => OrderDirection.Buy,
        OrderSide.Sell => OrderDirection.Sell,
        _ => OrderDirection.Unknown
    };

    private static OrderState ConvertOrderStatus(MatchUpdateType status) => status switch
    {
        MatchUpdateType.Open => OrderState.New,
        MatchUpdateType.Update => OrderState.Submitted,
        MatchUpdateType.Match => OrderState.PartFilled,
        MatchUpdateType.Filled => OrderState.Filled,
        MatchUpdateType.Canceled => OrderState.Cancelled,

        _ => OrderState.Unknown
    };
}
