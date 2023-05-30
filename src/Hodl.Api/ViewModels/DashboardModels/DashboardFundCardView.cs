using Hodl.Api.ViewModels.FundModels;

namespace Hodl.Api.ViewModels.DashboardModels;

public class DashboardFundCardView
{
    public Guid Id { get; set; }

    public FundOwnerListViewModel FundOwner { get; set; }

    public string FundName { get; set; }

    public string Description { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime? DateEnd { get; set; }

    public int MaxVolume { get; set; }

    public string LayerStrategy { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal TotalValue { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public int TotalShares { get; set; }

    public decimal ShareValueHWM { get; set; }

    public string CurrentBookingPeriod { get; set; }

    public virtual PeriodNavViewModel Nav { get; set; }

    public virtual ICollection<DashboardHoldingCardView> Holdings { get; set; }

    public virtual ICollection<DailyNavViewModel> DailyNavs { get; set; }

    public virtual ICollection<DashboardFundLayerCardView> Layers { get; set; }

    public virtual ICollection<string> CategoryGroups { get; set; }
}
