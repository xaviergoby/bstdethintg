namespace Hodl.Api.ViewModels.TradingModels;

public class OrderFundingEditViewModel
{
    public Guid FundId { get; set; }

    public decimal OrderPercentage { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal OrderAmount { get; set; }
}
