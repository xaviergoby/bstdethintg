namespace Hodl.Api.ViewModels.DashboardModels;

public class DashboardHoldingCardView
{
    public Guid Id { get; set; }

    public string CurrencySymbol { get; set; }

    public string CurrencyName { get; set; }

    public bool IsFiat { get; set; }

    public bool IsLocked { get; set; }

    public string BookingPeriod { get; set; }

    public DateTime StartDateTime { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal StartBalance { get; set; }

    public decimal StartUSDPrice { get; set; }

    public decimal StartBTCPrice { get; set; }

    public decimal StartPercentage { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal EndBalance { get; set; }

    public decimal EndUSDPrice { get; set; }

    public decimal EndBTCPrice { get; set; }

    public decimal EndPercentage { get; set; }

    public byte LayerIndex { get; set; }

    public string LayerName { get; set; } = "Undefined";
}
