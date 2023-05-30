using Hodl.Api.ViewModels.FundModels;

namespace Hodl.Api.ViewModels.ReportModels;

public class ReportFundViewModel
{
    public Guid Id { get; set; }

    public FundOwnerListViewModel FundOwner { get; set; }

    public string FundName { get; set; }

    public string Description { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime? DateEnd { get; set; }

    public int MaxVolume { get; set; }

    public string LayerStrategy { get; set; }

    public string ReportingCurrencyCode { get; set; }

    public virtual ReportPeriodNavViewModel Nav { get; set; }

    public virtual ICollection<ReportHoldingViewModel> Holdings { get; set; }

    public virtual ICollection<ReportFundLayerViewModel> Layers { get; set; }

    public virtual ICollection<ReportTradeSummaryViewModel> TradeSummary { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public virtual IEnumerable<ReportTransferListViewModel> TransferLog { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public virtual IEnumerable<ReportOrderLogViewModel> TradeLog { get; set; }
}
