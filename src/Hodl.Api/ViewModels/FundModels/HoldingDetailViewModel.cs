using Hodl.Api.ViewModels.CurrencyModels;

namespace Hodl.Api.ViewModels.FundModels;

public class HoldingDetailViewModel
{
    public Guid Id { get; set; }

    public Guid? PreviousHoldingId { get; set; }

    public CurrencyListViewModel Currency { get; set; }

    public CryptoCurrencyListViewModel CryptoCurrency { get; set; }

    public string BookingPeriod { get; set; }

    public DateTime PeriodClosedDateTime { get; set; }

    public DateTime StartDateTime { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal StartBalance { get; set; }

    public decimal StartUSDPrice { get; set; }

    public decimal StartBTCPrice { get; set; }

    public decimal StartPercentage { get; set; }

    public DateTime? EndDateTime { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal EndBalance { get; set; }

    public decimal EndUSDPrice { get; set; }

    public decimal EndBTCPrice { get; set; }

    public decimal EndPercentage { get; set; }

    public byte LayerIndex { get; set; }

    //public virtual ICollection<TransferListViewModel> Transfers { get; set; } = new HashSet<TransferListViewModel>();
}
