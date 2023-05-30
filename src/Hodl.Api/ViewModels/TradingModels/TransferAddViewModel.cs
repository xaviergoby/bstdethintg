using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.TradingModels;

public class TransferAddViewModel
{
    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    public TransactionType TransactionType { get; set; }

    [Required]
    public Guid FromHoldingId { get; set; }

    [Required, MaxLength(64)]
    public string FromAddress { get; set; }

    public Guid? ToHoldingId { get; set; }

    [MaxLength(64)]
    public string ToAddress { get; set; }

    [Required, MaxLength(128)]
    public string TransactionId { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal TransferAmount { get; set; }

    public decimal ExchangeRate { get; set; } = 1;

    [Required]
    public Guid FeeHoldingId { get; set; }

    public decimal TransferFee { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public int Shares { get; set; }

    public string Reference { get; set; }

    public bool IsCorrection { get; set; } = false;
}
