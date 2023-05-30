using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(HoldingId), nameof(BookingPeriod), nameof(DateTime))]
public class Transfer
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? OppositeTransferId { get; set; }

    [ForeignKey("OppositeTransferId")]
    public Transfer OppositeTransfer { get; set; }

    [Required]
    public Guid HoldingId { get; set; }

    [ForeignKey("HoldingId")]
    public Holding Holding { get; set; }

    [MinLength(6), MaxLength(6)]
    [Column(TypeName = "nchar(6)")]
    public string BookingPeriod { get; set; }

    public DateTime DateTime { get; set; }

    public TransactionType TransactionType { get; set; }

    /// <summary>
    /// The transaction source holds a reference to the wallet address or IBAN
    /// </summary>
    [MaxLength(64)]
    public string TransactionSource { get; set; }

    [MaxLength(128)]
    public string TransactionId { get; set; }

    public TransferDirection Direction { get; set; }

    public decimal TransferAmount { get; set; }

    [Required]
    public Guid FeeHoldingId { get; set; }

    [ForeignKey("FeeHoldingId")]
    public Holding FeeHolding { get; set; }

    public decimal TransferFee { get; set; }

    public int Shares { get; set; }

    public string Reference { get; set; }
}
