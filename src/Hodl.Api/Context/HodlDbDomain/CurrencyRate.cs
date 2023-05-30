using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(CurrencyISOCode), nameof(TimeStamp))]
public class CurrencyRate
{
    [Key]
    public long Id { get; set; }

    [Required, MaxLength(3)]
    public string CurrencyISOCode { get; set; }

    [ForeignKey("CurrencyISOCode")]
    public Currency Currency { get; set; }

    [Required]
    public decimal USDRate { get; set; }

    public DateTime TimeStamp { get; set; }

    [MaxLength(128)]
    public string Source { get; set; }
}
