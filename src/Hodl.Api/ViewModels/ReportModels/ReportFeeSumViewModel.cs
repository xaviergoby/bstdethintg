namespace Hodl.Api.ViewModels.ReportModels;

public class ReportFeeSumViewModel
{
    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal TotalFee { get; set; }

    public string FeeCryptoSymbol { get; set; }

    public string FeeCryptoName { get; set; }
}
