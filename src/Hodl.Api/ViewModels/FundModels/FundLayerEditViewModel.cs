using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.FundModels;

public class FundLayerEditViewModel
{
    [Required(ErrorMessage = "The fund-layer must have an identiying name"), MaxLength(40)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public byte AimPercentage { get; set; }

    public byte AlertRangeLow { get; set; }

    public byte AlertRangeHigh { get; set; }
}
