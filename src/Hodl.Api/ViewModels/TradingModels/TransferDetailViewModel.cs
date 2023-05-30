using Hodl.Api.ViewModels.FundModels;

namespace Hodl.Api.ViewModels.TradingModels;

public class TransferDetailViewModel
{
    public HoldingListViewModel Holding { get; set; }

    public TransferListViewModel OppositeTransfer { get; set; }

    public string BookingPeriod { get; set; }

    public DateTime DateTime { get; set; }

    public TransactionType TransactionType { get; set; }

    public string TransactionSource { get; set; }

    public string TransactionId { get; set; }

    public TransferDirection Direction { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal TransferAmount { get; set; }

    public decimal ExchangeRate { get; set; } = 1;

    public HoldingListViewModel FeeHolding { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal TransferFee { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public int Shares { get; set; }

    public string Reference { get; set; }
}
