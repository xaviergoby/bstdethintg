namespace Hodl.Api.ViewModels.DashboardModels;

public class DashboardFundCategoryCardView
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Group { get; set; }

    public byte MinPercentage { get; set; }

    public byte MaxPercentage { get; set; }

    public int NumberOfItems { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal USDValue { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal BTCValue { get; set; }

    public decimal AllocationPercentage { get; set; }
}
