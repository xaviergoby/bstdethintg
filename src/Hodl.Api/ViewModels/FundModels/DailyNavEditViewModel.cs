namespace Hodl.Api.ViewModels.FundModels;

public class DailyNavEditViewModel
{
    public DateTime DateTime { get; set; }

    public DateTime Date { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal TotalValue { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public int TotalShares { get; set; }

    public decimal ShareHWM { get; set; }

    public decimal ShareGross { get; set; }

    public decimal ShareNAV { get; set; }

    public long CurrencyRateId { get; set; }
}
