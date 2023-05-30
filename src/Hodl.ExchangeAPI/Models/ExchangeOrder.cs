using Hodl.ExchangeAPI.Constants;

namespace Hodl.ExchangeAPI.Models;

public class ExchangeOrder
{
    public string OrderId { get; set; }

    public string ClientOrderId { get; set; }

    public string WalletAddress { get; set; }

    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The Base Asset is the first currency in the pair. It is the asset to 
    /// buy or sell. The Amount is the quantity of this asset you want to buy.
    /// </summary>
    public ExchangeCryptoCurrency BaseAsset { get; set; }

    /// <summary>
    /// The Quote Asset is the last currency in the pair. It is the currency to 
    /// pay the base asset with in case of a buy, or the currency to retrieve 
    /// in case of a sell. It is the result of the ammount multiplied by the 
    /// price.
    /// </summary>
    public ExchangeCryptoCurrency QuoteAsset { get; set; }

    public string Type { get; set; }

    public OrderDirection Direction { get; set; }

    public OrderState State { get; set; } = OrderState.New;

    public decimal UnitPrice { get; set; }

    public decimal Amount { get; set; }

    public decimal Total { get; set; }

    public bool IsMaker { get; set; } = false;

    public bool IsTaker { get; set; } = true;
}
