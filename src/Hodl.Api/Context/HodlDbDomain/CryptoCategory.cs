using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(CryptoId), nameof(CategoryId), IsUnique = true)]
public class CryptoCategory
{
    [Required]
    public Guid CryptoId { get; set; }

    [ForeignKey("CryptoId")]
    public CryptoCurrency CryptoCurrency { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public Category Category { get; set; }
}
