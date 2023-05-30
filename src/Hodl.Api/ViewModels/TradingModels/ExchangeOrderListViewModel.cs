namespace Hodl.Api.ViewModels.TradingModels;

public class ExchangeOrderListViewModel
{
    public string OrderId { get; set; }

    public string ClientOrderId { get; set; }

    public Guid? InternalOrderId { get; set; }

    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The Base Asset is the first currency in the pair. It is the asset to 
    /// buy or sell. The Amount is the quantity of this asset you want to buy.
    /// </summary>
    public string BaseAsset { get; set; }

    /// <summary>
    /// The Quote Asset is the last currency in the pair. It is the currency to 
    /// pay the base asset with in case of a buy, or the currency to retrieve 
    /// in case of a sell. It is the result of the ammount multiplied by the 
    /// price.
    /// </summary>
    public string QuoteAsset { get; set; }

    public string Type { get; set; }

    public OrderDirection Direction { get; set; }

    public OrderState State { get; set; } = OrderState.New;

    public decimal UnitPrice { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal Amount { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal Total { get; set; }
}
