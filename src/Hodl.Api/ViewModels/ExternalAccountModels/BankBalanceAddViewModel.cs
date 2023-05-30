using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.ExternalAccountModels;

public class BankBalanceAddViewModel
{
    [Required]
    public Guid BankAccountId { get; set; }

    [Required, MaxLength(3)]
    public string CurrencyISOCode { get; set; }

    [Required]
    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal Balance { get; set; }

    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}
