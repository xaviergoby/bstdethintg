using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.ExternalAccountModels;

public class BankEditViewModel
{
    [Required, MaxLength(60)]
    public string Name { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(11)]
    public string BIC { get; set; } = string.Empty;

    [MaxLength(60)]
    public string City { get; set; }

    [MaxLength(60)]
    public string Branch { get; set; }

    [MaxLength(120)]
    public string Address { get; set; }

    [MaxLength(10)]
    public string Zipcode { get; set; }

    [MaxLength(60)]
    public string Country { get; set; }

    [MaxLength(2)]
    public string CountryCode { get; set; }

    [MaxLength(256)]
    public string Url { get; set; }
}
