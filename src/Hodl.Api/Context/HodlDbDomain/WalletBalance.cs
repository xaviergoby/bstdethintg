using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(Address), nameof(CryptoId), nameof(BlockchainNetworkId), nameof(Timestamp))]
[Index(nameof(CryptoId), nameof(BlockchainNetworkId))]
public class WalletBalance
{
    [Key]
    public Guid Id { get; set; }

    [Required, MaxLength(128)]
    public string Address { get; set; }

    [ForeignKey("Address")]
    public Wallet Wallet { get; set; }

    [Required]
    public Guid CryptoId { get; set; }

    [ForeignKey("CryptoId")]
    public CryptoCurrency CryptoCurrency { get; set; }

    public Guid? BlockchainNetworkId { get; set; }

    [ForeignKey("BlockchainNetworkId")]
    public BlockchainNetwork BlockchainNetwork { get; set; }

    public DateTime Timestamp { get; set; }

    [Required]
    public decimal Balance { get; set; }

    public WalletBalance()
    {
        Id = Guid.NewGuid();
    }
}
