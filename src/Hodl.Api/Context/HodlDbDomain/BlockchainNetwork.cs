using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.HodlDbDomain;

public class BlockchainNetwork
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(128)]
    public string RPCUrl { get; set; }

    public long ChainID { get; set; }

    [MaxLength(5)]
    public string CurrencySymbol { get; set; }

    [MaxLength(256)]
    public string ExplorerUrl { get; set; }

    public bool IsTestNet { get; set; } = false;

    public virtual ICollection<TokenContract> TokenContracts { get; set; }

    public virtual ICollection<WalletBalance> WalletBalances { get; set; }

    public override string ToString() => Name;
}
