using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(FundId), nameof(DateTime))]
public class Nav
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid FundId { get; set; }

    [ForeignKey("FundId")]
    public Fund Fund { get; set; }

    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    public NavType Type { get; set; }

    [MinLength(6), MaxLength(6)]
    [Column(TypeName = "nchar(6)")]
    public string BookingPeriod { get; set; }

    public DateTime Date { get; set; }

    public decimal TotalValue { get; set; }

    public int TotalShares { get; set; }

    public decimal ShareHWM { get; set; }

    public decimal ShareGross { get; set; }

    public decimal ShareNAV { get; set; }

    public decimal AdministrationFee { get; set; } = 0;

    public decimal PerformanceFee { get; set; } = 0;

    public decimal InOutValue { get; set; }

    public int InOutShares { get; set; }

    public long CurrencyRateId { get; set; }

    [ForeignKey("CurrencyRateId")]
    public CurrencyRate CurrencyRate { get; set; }
}
