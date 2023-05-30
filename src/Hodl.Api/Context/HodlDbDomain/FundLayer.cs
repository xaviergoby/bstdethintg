using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(FundId), nameof(LayerIndex))]
public class FundLayer
{
    [Required]
    public Guid FundId { get; set; }

    [ForeignKey("FundId")]
    public Fund Fund { get; set; }

    [Required]
    public byte LayerIndex { get; set; }

    [Required, MaxLength(40)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public byte AimPercentage { get; set; }

    public byte AlertRangeLow { get; set; }

    public byte AlertRangeHigh { get; set; }
}
