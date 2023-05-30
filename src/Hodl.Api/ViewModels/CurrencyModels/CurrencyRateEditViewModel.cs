using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.CurrencyModels;

public class CurrencyRateEditViewModel
{
    [Required]
    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal USDRate { get; set; }

    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    [MaxLength(128)]
    public string Source { get; set; }
}
