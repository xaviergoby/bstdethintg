using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.HodlDbDomain;

public class Exchange
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(60)]
    public string ExchangeName { get; set; }

    [MaxLength(256)]
    public string Url { get; set; }

    public byte[] Icon { get; set; }

    public bool IsDefi { get; set; }

    public virtual ICollection<ExchangeAccount> ExchangeAccounts { get; set; }

    public override string ToString() => ExchangeName;
}
