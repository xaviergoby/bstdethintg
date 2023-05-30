using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

public class Wallet
{
    [Key, MaxLength(128)]
    public string Address { get; set; }

    public Guid? ExchangeAccountId { get; set; }

    [ForeignKey("ExchangeAccountId")]
    public ExchangeAccount ExchangeAccount { get; set; }

    public DateTime Timestamp { get; set; }

    public string Description { get; set; }

    public virtual ICollection<WalletBalance> WalletBalances { get; set; }

    public virtual ICollection<Order> Orders { get; set; }
}
