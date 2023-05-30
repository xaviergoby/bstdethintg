using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.TradingModels;

public class TransferEditViewModel
{
    public DateTime DateTime { get; set; } = DateTime.MinValue;

    [MaxLength(64)]
    public string FromAddress { get; set; }

    [MaxLength(64)]
    public string ToAddress { get; set; }

    [MaxLength(128)]
    public string TransactionId { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal TransferAmount { get; set; }

    public decimal ExchangeRate { get; set; } = 1;

    [Required]
    public Guid FeeHoldingId { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal TransferFee { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public int Shares { get; set; }

    public string Reference { get; set; }
}
