using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(IBAN), IsUnique = true)]
public class BankAccount
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(34)]
    public string IBAN { get; set; }

    [Required]
    public Guid BankId { get; set; }

    [ForeignKey("BankId")]
    public Bank Bank { get; set; }

    [MaxLength(100)]
    public string HolderName { get; set; }

    [Required]
    public Guid FundId { get; set; }

    [ForeignKey("FundId")]
    public Fund Fund { get; set; }

    public virtual ICollection<BankBalance> Balances { get; set; }

    public override string ToString() => $"{IBAN} : {HolderName}";
}
