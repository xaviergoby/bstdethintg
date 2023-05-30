using Hodl.Api.ViewModels.FundModels;

namespace Hodl.Api.ViewModels.DashboardModels;

public class DashboardFundLayerCardView : FundLayerViewModel
{
    public int NumberOfHoldings { get; set; }

    public decimal CurrentPercentage { get; set; }
}
