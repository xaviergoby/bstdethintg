namespace Hodl.Api.ViewModels.ReportModels;

public class ReportHoldingViewModel
{
    public Guid Id { get; set; }

    public string CurrencySymbol { get; set; }

    public string CurrencyName { get; set; }

    public bool IsFiat { get; set; }

    public bool IsLocked { get; set; }

    public DateTime StartDateTime { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal StartBalance { get; set; }

    public decimal StartUSDPrice { get; set; }

    public decimal StartBTCPrice { get; set; }

    public decimal StartPercentage { get; set; }

    public decimal EndUSDPrice { get; set; }

    public decimal EndBTCPrice { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal NavBalance { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal NavUSDValue { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal NavBTCValue { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal EndBalance { get; set; }

    public decimal EndPercentage { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal EndUSDValue { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal EndBTCValue { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal EndReportingCurrencyValue { get; set; }

    public byte LayerIndex { get; set; }

    public string LayerName { get; set; } = "Undefined";
}
