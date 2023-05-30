namespace Hodl.Api.ViewModels.ReportModels;

public class ReportTransferListViewModel
{
    public string CurrencySymbol { get; set; }

    public string CurrencyName { get; set; }

    public DateTime DateTime { get; set; }

    public TransactionType TransactionType { get; set; }

    public string TransactionSource { get; set; }

    public string TransactionId { get; set; }

    public TransferDirection Direction { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal TransferAmount { get; set; }

    public string FeeCurrencySymbol { get; set; }

    public string FeeCurrencyName { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal TransferFee { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public int Shares { get; set; }

    public string Reference { get; set; }
}
