using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(ExchangeId), nameof(AccountKey), IsUnique = true)]
public class ExchangeAccount
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ExchangeId { get; set; }

    [ForeignKey("ExchangeId")]
    public Exchange Exchange { get; set; }

    [MaxLength(40)]
    public string Name { get; set; }

    [MaxLength(128)]
    public string AccountKey { get; set; }

    [MaxLength(128)]
    public string AccountSecret { get; set; }

    public string AccountPrivateKey { get; set; }

    public Guid? ParentAccountId { get; set; }

    [ForeignKey("ParentAccountId")]
    public ExchangeAccount ParentAccount { get; set; }

    public virtual ICollection<ExchangeAccount> ChildAccounts { get; set; }

    public virtual ICollection<Order> Orders { get; set; }

    public virtual ICollection<Wallet> Wallets { get; set; }

    public override string ToString() => Name;
}
