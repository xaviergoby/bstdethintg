using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Spot;
using Binance.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Sockets;
using Hodl.ExchangeAPI.Constants;
using Hodl.ExchangeAPI.Models;
using Microsoft.Extensions.Logging;

namespace Hodl.ExchangeAPI.Binance;

public class ExchangeBinance : BaseExchangeAPI
{
    // Test Binance
    private readonly BinanceClient _binanceClient;
    private readonly BinanceSocketClient _binanceSocketClient;

    private string _userStreamListenKey = string.Empty;

    public ExchangeBinance(Guid accountId, string key, string secret, bool testEnvironment) : base(accountId, key)
    {
        var apiCredentials = new BinanceApiCredentials(key, secret);
        _binanceClient = new BinanceClient(new BinanceClientOptions
        {
            ApiCredentials = apiCredentials,
            SpotApiOptions = new BinanceApiClientOptions()
            {
                BaseAddress = testEnvironment
                    ? "https://testnet.binance.vision"
                    : "https://api.binance.com",
            },
            LogLevel = LogLevel.Debug
        });

        _binanceSocketClient = new BinanceSocketClient(new BinanceSocketClientOptions
        {
            ApiCredentials = apiCredentials,
            SpotApiOptions = new BinanceSocketApiClientOptions
            {
                BaseAddress = testEnvironment
                    ? "wss://testnet.binance.vision"
                    : "wss://stream.binance.com:9443",
                AutoReconnect = true,
                ApiCredentials = apiCredentials,
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
                Task.Run(async () => { await UnSubscribeOrderUpdates(); }).Wait();

                // Disconnect the API
                _binanceSocketClient.Dispose();
                _binanceClient.Dispose();
            }

            base._disposedValue = true;
        }
    }
    #endregion

    public override async Task<IEnumerable<ExchangeOrder>> GetOrders(string symbol, DateTime startTime, CancellationToken cancelationToken = default)
    {
        // Binance only allows fetching orders by symbol
        if (string.IsNullOrEmpty(symbol))
            throw new ArgumentNullException(nameof(symbol));

        string binanceSymbol = symbol.Replace("/", "");

        var webResult = await _binanceClient.SpotApi.Trading.GetOrdersAsync(binanceSymbol, startTime: startTime, ct: cancelationToken);

        if (webResult.Success)
        {
            return webResult.Data
                .Select(o => OrderFromBinanceOrder(o))
                .ToList();
        }

        return new List<ExchangeOrder>();
    }

    public override async Task<ExchangeOrder> GetOrder(string symbol, string orderId, CancellationToken cancelationToken = default)
    {
        string binanceSymbol = symbol.Replace("/", "");
        long binanceOrderId = long.Parse(orderId);

        var webResult = await _binanceClient.SpotApi.Trading.GetOrderAsync(binanceSymbol, orderId: binanceOrderId, ct: cancelationToken);

        if (webResult.Success)
        {
            return OrderFromBinanceOrder(webResult.Data);
        }

        throw new KeyNotFoundException($"Exchange order {orderId} on Binance not found");
    }

    public override async Task<IEnumerable<ExchangeTrade>> GetOrderTrades(string symbol, string orderId, CancellationToken cancelationToken = default)
    {
        string binanceSymbol = symbol.Replace("/", "");
        long binanceOrderId = long.Parse(orderId);

        var webResult = await _binanceClient.SpotApi.Trading.GetUserTradesAsync(binanceSymbol, orderId: binanceOrderId, ct: cancelationToken);

        if (webResult.Success)
        {
            return webResult.Data
                .Select(t => TradeFromBinanceTrade(t))
                .ToList();
        }

        return new List<ExchangeTrade>();
    }

    public override async Task<bool> SubscribeOrderUpdates(CancellationToken cancelationToken = default)
    {
        // Add event handlers for orders and trades
        var webResult = await _binanceClient.SpotApi.Account.StartUserStreamAsync(cancelationToken);

        if (!webResult.Success)
        {
            // Handler failure
            return false;
        }

        _userStreamListenKey = webResult.Data;

        var sub = await _binanceSocketClient.SpotApi.Account.SubscribeToUserDataUpdatesAsync(_userStreamListenKey,
            OnOrderUpdateMessage,
            data =>
            {
                // Handle oco order update
            },
            data =>
            {
                // Handle account balance update, caused by trading
            },
            data =>
            {
                // Handle account balance update, caused by withdrawal/deposit or transfers
            },
            cancelationToken);

        return sub.Success;
    }

    public override async Task<IEnumerable<ExchangeBalance>> GetBalances(CancellationToken cancelationToken = default)
    {
        var webResult = await _binanceClient.SpotApi.Account.GetAccountInfoAsync(ct: cancelationToken);

        if (webResult.Success)
        {
            return webResult.Data
                .Balances
                .Select(b => BalanceFromBinanceUserBalance(b))
                .ToList();
        }

        return new List<ExchangeBalance>();
    }

    private async Task UnSubscribeOrderUpdates()
    {
        await _binanceClient.SpotApi.Account.StopUserStreamAsync(_userStreamListenKey);
    }

    private void OnOrderUpdateMessage(DataEvent<BinanceStreamOrderUpdate> data)
    {
        // Handle order update
        try
        {
            switch (data.Data.ExecutionType)
            {
                case ExecutionType.Trade:
                    base.OnNewTrade(TradeFromData(data.Data));
                    break;
                case ExecutionType.New:
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

    private ExchangeOrder OrderFromBinanceOrder(BinanceOrder order) => new()
    {
        OrderId = order.Id.ToString(),
        ClientOrderId = order.ClientOrderId,
        DateTime = order.CreateTime,
        BaseAsset = GetBaseAsset(order.Symbol),
        QuoteAsset = GetQuoteAsset(order.Symbol),
        Type = order.Type.ToString(),
        Direction = ConvertOrderSide(order.Side),
        State = ConvertOrderStatus(order.Status),
        UnitPrice = order.Price,
        Amount = order.Quantity,
        Total = order.Quantity * order.Price,
        IsMaker = false,
        IsTaker = true
    };

    private ExchangeOrder OrderFromData(BinanceStreamOrderUpdate order) => new()
    {
        OrderId = order.Id.ToString(),
        ClientOrderId = order.ClientOrderId,
        DateTime = order.CreateTime,
        BaseAsset = GetBaseAsset(order.Symbol),
        QuoteAsset = GetQuoteAsset(order.Symbol),
        Type = order.Type.ToString(),
        Direction = ConvertOrderSide(order.Side),
        State = ConvertOrderStatus(order.Status),
        UnitPrice = order.Price,
        Amount = order.Quantity,
        Total = order.QuoteQuantity,
        IsMaker = order.BuyerIsMaker,
        IsTaker = !order.BuyerIsMaker
    };

    private static ExchangeTrade TradeFromBinanceTrade(BinanceTrade trade) => new()
    {
        Id = trade.Id.ToString(),
        OrderId = trade.OrderId.ToString(),
        TransactionId = "-",
        DateTime = trade.Timestamp,
        UnitPrice = trade.Price,
        Executed = trade.Quantity,
        Total = trade.QuoteQuantity,
        Fee = trade.Fee,
        FeeAsset = GetFeeAsset(trade.FeeAsset)
    };

    private static ExchangeTrade TradeFromData(BinanceStreamOrderUpdate order) => new()
    {
        Id = order.TradeId.ToString(),
        OrderId = order.Id.ToString(),
        TransactionId = "-",
        DateTime = order.UpdateTime,
        UnitPrice = order.LastPriceFilled,
        Executed = order.LastQuantityFilled,
        Total = order.LastQuoteQuantity,
        Fee = order.Fee,
        FeeAsset = GetFeeAsset(order.FeeAsset)
    };

    private static ExchangeBalance BalanceFromBinanceUserBalance(BinanceBalance balance) => new()
    {
        Asset = balance.Asset,
        Name = balance.Asset,
        Available = balance.Available,
        Locked = balance.Locked,
        Total = balance.Total
    };

    private async Task<BinanceSymbol> GetSpotSymbol(string symbol)
    {
        if (await UpdateMarketData())
        {
            var binanceSymbol = spotInfo?.Symbols.FirstOrDefault(s => s.Name.Equals(symbol));

            if (binanceSymbol == null && await UpdateMarketData(forceUpdate: true))
            {
                binanceSymbol = spotInfo?.Symbols.FirstOrDefault(s => s.Name.Equals(symbol));
            }

            if (binanceSymbol != null)
                return binanceSymbol;
        }

        throw new Exception($"Binance symbol not found: {symbol}");
    }

    private ExchangeCryptoCurrency GetBaseAsset(string symbol)
    {
        var binanceSymbol = Task.Run(async () => await GetSpotSymbol(symbol)).Result;

        return new ExchangeCryptoCurrency
        {
            Name = binanceSymbol.BaseAsset,
            Symbol = binanceSymbol.BaseAsset,
        };
    }

    private ExchangeCryptoCurrency GetQuoteAsset(string symbol)
    {
        var binanceSymbol = Task.Run(async () => await GetSpotSymbol(symbol)).Result;

        return new ExchangeCryptoCurrency
        {
            Name = binanceSymbol.QuoteAsset,
            Symbol = binanceSymbol.QuoteAsset
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
    private static BinanceExchangeInfo spotInfo;

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
                var webCallResult = await _binanceClient.SpotApi.ExchangeData.GetExchangeInfoAsync(cancellationToken);

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

    private static OrderState ConvertOrderStatus(OrderStatus status) => status switch
    {
        OrderStatus.New => OrderState.New,
        OrderStatus.PartiallyFilled => OrderState.PartFilled,
        OrderStatus.Filled => OrderState.Filled,
        OrderStatus.Canceled => OrderState.Cancelled,
        OrderStatus.PendingCancel => OrderState.CancelPending,
        OrderStatus.Rejected => OrderState.Rejected,
        OrderStatus.Expired => OrderState.Expired,
        OrderStatus.Insurance => OrderState.Unknown,
        OrderStatus.Adl => OrderState.Unknown,

        _ => OrderState.Unknown
    };
}
