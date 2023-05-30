namespace Hodl.Api.ViewModels.ReportModels;

public class ReportTradeLogViewModel
{
    public string TransactionId { get; set; }

    public DateTime DateTime { get; set; }

    public decimal UnitPrice { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal Executed { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal Total { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal Fee { get; set; }

    public string FeeCurrencySymbol { get; set; }
    public string FeeCurrencyName { get; set; }
}
