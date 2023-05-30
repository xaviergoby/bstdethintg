using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(FundId), nameof(BookingPeriod), nameof(CurrencyISOCode))]
public class Holding
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? PreviousHoldingId { get; set; }

    [ForeignKey("PreviousHoldingId")]
    public Holding PreviousHolding { get; set; }

    public Holding NextHolding { get; set; }

    [Required]
    public Guid FundId { get; set; }

    [ForeignKey("FundId")]
    public Fund Fund { get; set; }

    [MaxLength(3)]
    public string CurrencyISOCode { get; set; }

    [ForeignKey("CurrencyISOCode")]
    public Currency Currency { get; set; }

    public long? CurrencyRateId { get; set; }

    [ForeignKey("CurrencyRateId")]
    public CurrencyRate CurrencyRate { get; set; }

    public Guid? CryptoId { get; set; }

    [ForeignKey("CryptoId")]
    public CryptoCurrency CryptoCurrency { get; set; }

    public long? ListingId { get; set; }

    [ForeignKey("ListingId")]
    public Listing Listing { get; set; }

    public Guid? SharesFundId { get; set; }

    [ForeignKey("SharesFundIdFundId")]
    public Fund SharesFund { get; set; }

    [Required, MinLength(6), MaxLength(6)]
    [Column(TypeName = "nchar(6)")]
    public string BookingPeriod { get; set; } = DateTime.Now.ToString("yyyymm");

    public DateTime? PeriodClosedDateTime { get; set; }

    public DateTime StartDateTime { get; set; } = DateTime.UtcNow;

    public decimal StartBalance { get; set; } = 0;

    public decimal StartUSDPrice { get; set; }

    public decimal StartBTCPrice { get; set; }

    public decimal StartPercentage { get; set; } = 0;

    public DateTime? EndDateTime { get; set; }

    public decimal NavBalance { get; set; }

    public decimal EndBalance { get; set; }

    public decimal EndUSDPrice { get; set; }

    public decimal EndBTCPrice { get; set; }

    public decimal EndPercentage { get; set; }

    public byte LayerIndex { get; set; }

    public virtual ICollection<Transfer> Transfers { get; set; } = new HashSet<Transfer>();
}
