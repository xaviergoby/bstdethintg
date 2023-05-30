using Hodl.Api.ViewModels.FundModels;
using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.TradingModels;

public class TransferListViewModel
{
    [Key]
    public Guid Id { get; set; }

    public HoldingListViewModel Holding { get; set; }

    public string BookingPeriod { get; set; }

    public DateTime DateTime { get; set; }

    public TransactionType TransactionType { get; set; }

    public string TransactionSource { get; set; }

    public string TransactionId { get; set; }

    public TransferDirection Direction { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal TransferAmount { get; set; }

    public decimal ExchangeRate { get; set; }

    public HoldingListViewModel FeeHolding { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal TransferFee { get; set; }

    public string Reference { get; set; }
}
