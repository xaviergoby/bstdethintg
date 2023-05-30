namespace Hodl.Api.ViewModels.ReportModels;

public class ReportOrderLogViewModel
{
    public string Exchange { get; set; }

    public string ExchangeAccount { get; set; }

    public string WalletAddress { get; set; }

    public string OrderNumber { get; set; }

    public DateTime DateTime { get; set; }

    public string BaseAssetSymbol { get; set; }

    public string BaseAssetName { get; set; }

    public string QuoteAssetSymbol { get; set; }

    public string QuoteAssetName { get; set; }

    public string Type { get; set; }

    public OrderDirection Direction { get; set; }

    public OrderState State { get; set; }

    public decimal UnitPrice { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal Amount { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal Total { get; set; }

    public decimal FundingPercentage { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal FilledAmount { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal FilledTotal { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public ICollection<ReportFeeSumViewModel> TotalFees { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal FundAmount { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal FundTotal { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public ICollection<ReportFeeSumViewModel> FundFees { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public IList<ReportTradeLogViewModel> Trades { get; } = new List<ReportTradeLogViewModel>();
}
