using Hodl.Api.ViewModels.FundModels;

namespace Hodl.Api.ViewModels.ReportModels;

public class ReportFundLayerViewModel : FundLayerViewModel
{
    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public int NumberOfHoldings { get; set; }

    public decimal CurrentPercentage { get; set; }
}