using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.TradingModels;

public class OrderAddViewModel
{
    public Guid? ExchangeAccountId { get; set; }

    [MaxLength(64)]
    public string WalletAddress { get; set; }

    [MaxLength(64)]
    public string OrderNumber { get; set; }

    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    public Guid BaseAssetId { get; set; }

    public Guid QuoteAssetId { get; set; }

    [MaxLength(20)]
    public string Type { get; set; }

    public OrderDirection Direction { get; set; }

    public decimal UnitPrice { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal Amount { get; set; }

    public OrderFundingEditViewModel[] OrderFundings { get; set; }
}
