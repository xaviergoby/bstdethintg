namespace Hodl.Api.ViewModels.ReportModels;

public class ReportTradeSummaryViewModel : ReportHoldingViewModel
{
    // Inflow outflow sum
    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal InOutFlow { get; set; }

    // Inflow outflow shares sum
    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public int InOutFlowShares { get; set; }

    // The sum of all the trades on the holding
    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal TradeSum { get; set; }

    // Potential rewards
    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal StakingRewards { get; set; }

    // Sum of payd fees
    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal Fees { get; set; }

    // Sum of profit and loss
    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal ProfitAndLoss { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal CalculatedEndValue { get; set; }
}
