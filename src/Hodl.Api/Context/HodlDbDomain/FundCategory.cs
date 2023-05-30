using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(FundId), nameof(CategoryId), IsUnique = true)]
public class FundCategory
{
    [Required]
    public Guid FundId { get; set; }

    [ForeignKey("FundId")]
    public Fund Fund { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public Category Category { get; set; }

    public byte MinPercentage { get; set; }

    public byte MaxPercentage { get; set; }
}
