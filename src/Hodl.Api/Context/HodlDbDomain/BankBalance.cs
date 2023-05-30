using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(BankAccountId), nameof(CurrencyISOCode), IsUnique = true)]
[Index(nameof(BankAccountId), nameof(CurrencyISOCode), nameof(TimeStamp))]
public class BankBalance
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BankAccountId { get; set; }

    [ForeignKey("BankAccountId")]
    public BankAccount BankAccount { get; set; }

    [Required, MaxLength(3)]
    public string CurrencyISOCode { get; set; }

    [ForeignKey("CurrencyISOCode")]
    public Currency Currency { get; set; }

    public decimal Balance { get; set; }

    public DateTime TimeStamp { get; set; }

    public override string ToString() => $"{CurrencyISOCode} : {Balance}";
}
