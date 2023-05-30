using Hodl.Api.ViewModels.CurrencyModels;
using Hodl.Api.ViewModels.ExternalAccountModels;
using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.TradingModels;

public class OrderDetailViewModel
{
    public ExchangeAccountListViewModel ExchangeAccount { get; set; }

    [MaxLength(64)]
    public string WalletAddress { get; set; }

    [MaxLength(64)]
    public string OrderNumber { get; set; }

    public bool ImportedFromExchange { get; set; } = false;

    public CryptoCurrencyListViewModel BaseAsset { get; set; }

    public CryptoCurrencyListViewModel QuoteAsset { get; set; }

    public DateTime DateTime { get; set; }

    public string Type { get; set; }

    public OrderDirection Direction { get; set; }

    public OrderState State { get; set; }

    public decimal UnitPrice { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal Amount { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal Total { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal Executed { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal TotalCost { get; set; }

    public decimal AveragePrice { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public ICollection<FeeSumViewModel> TotalFees { get; set; }

    public virtual ICollection<OrderFundingListViewModel> OrderFundings { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public virtual ICollection<TradeListViewModel> Trades { get; set; }
}
