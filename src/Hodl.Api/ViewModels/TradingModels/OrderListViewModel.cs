using Hodl.Api.ViewModels.CurrencyModels;
using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.TradingModels;

public class OrderListViewModel
{
    [Key]
    public Guid Id { get; set; }

    public string Exchange { get; set; }

    public string ExchangeAccount { get; set; }

    [MaxLength(64)]
    public string WalletAddress { get; set; }

    [MaxLength(64)]
    public string OrderNumber { get; set; }

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

    public ICollection<OrderFundingListViewModel> OrderFundings { get; set; }
}
