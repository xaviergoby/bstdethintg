using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.CurrencyModels;

public class CurrencyRateViewModel
{
    [Key]
    public long Id { get; set; }

    [Required, MaxLength(3)]
    public string CurrencyISOCode { get; set; }

    [Required(ErrorMessage = "The USD exchange rate is required")]
    public decimal USDRate { get; set; }

    public DateTime TimeStamp { get; set; }

    [MaxLength(128)]
    public string Source { get; set; }
}
