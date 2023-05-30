namespace Hodl.Api.ViewModels.TradingModels;

public class OrderFundingListViewModel
{
    public string FundName { get; set; }

    public decimal OrderPercentage { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal OrderAmount { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal OrderTotal { get; set; }
}
