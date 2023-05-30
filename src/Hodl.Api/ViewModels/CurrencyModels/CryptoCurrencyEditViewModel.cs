using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.CurrencyModels;

public class CryptoCurrencyEditViewModel
{
    public byte Decimals { get; set; } = 9;

    [Required(ErrorMessage = "A symbol for the token is required"), MinLength(2), MaxLength(8)]
    public string Symbol { get; set; }

    [Required(ErrorMessage = "An identifying name is required"), MinLength(2), MaxLength(40)]
    public string Name { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public bool Active { get; set; } = true;

    public bool IsStableCoin { get; set; } = false;

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public bool IsLocked { get; set; } = false;

    public string Icon { get; set; }

    public Guid? ListingCryptoId { get; set; }
}
