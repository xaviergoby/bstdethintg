namespace Hodl.Api.ViewModels.ReportModels;

public class ReportPeriodNavViewModel
{
    public Guid Id { get; set; }

    public DateTime DateTime { get; set; }

    public string BookingPeriod { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal TotalValue { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public int TotalShares { get; set; }

    public decimal ShareHWM { get; set; }

    public decimal ShareGross { get; set; }

    public decimal ShareNAV { get; set; }

    public decimal AdministrationFee { get; set; }

    public decimal PerformanceFee { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal InOutValue { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public int InOutShares { get; set; }

    public ReportCurrencyRateViewModel CurrencyRate { get; set; }
}
