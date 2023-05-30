namespace Hodl.Api.ViewModels.FundModels;

public class PeriodNavEditViewModel
{
    public DateTime DateTime { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal TotalValue { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public int TotalShares { get; set; }

    public decimal ShareHWM { get; set; }

    public decimal ShareGross { get; set; }

    public decimal ShareNAV { get; set; }
}
