namespace Hodl.Api.ViewModels.FundModels;

public class HoldingEditViewModel
{
    public DateTime StartDateTime { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal StartBalance { get; set; }

    public decimal StartUSDPrice { get; set; }

    public decimal StartBTCPrice { get; set; }

    public DateTime? EndDateTime { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal EndBalance { get; set; }

    public decimal EndUSDPrice { get; set; }

    public decimal EndBTCPrice { get; set; }

    public byte LayerIndex { get; set; }
}
