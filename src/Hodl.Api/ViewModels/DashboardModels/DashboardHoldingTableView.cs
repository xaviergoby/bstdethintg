using Hodl.Api.ViewModels.FundModels;

namespace Hodl.Api.ViewModels.DashboardModels;

public class DashboardHoldingTableView : DashboardHoldingCardView
{
    public decimal EndUSDValue { get; set; }

    public decimal EndBTCValue { get; set; }

    public FundCategoryViewModel[] Categories { get; set; }
}
