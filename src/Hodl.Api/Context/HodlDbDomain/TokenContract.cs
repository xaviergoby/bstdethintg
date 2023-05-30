using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(CryptoId), nameof(BlockchainNetworkId), nameof(ContractAddress), IsUnique = true)]
public class TokenContract
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CryptoId { get; set; }

    [ForeignKey("CryptoId")]
    public CryptoCurrency CryptoCurrency { get; set; }

    [Required]
    public Guid BlockchainNetworkId { get; set; }

    [ForeignKey("BlockchainNetworkId")]
    public BlockchainNetwork BlockchainNetwork { get; set; }

    [Required, MaxLength(128)]
    public string ContractAddress { get; set; }
}
