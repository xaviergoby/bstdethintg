using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.TradingModels;

public class OrderEditViewModel
{
    [MaxLength(64)]
    public string OrderNumber { get; set; }

    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string Type { get; set; }

    public OrderDirection Direction { get; set; }

    public OrderState State { get; set; }

    public decimal UnitPrice { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal Amount { get; set; }

    public OrderFundingEditViewModel[] OrderFundings { get; set; }
}
