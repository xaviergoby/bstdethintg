using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.FundModels;

public class FundClosePeriodViewModel
{
    [Required, MaxLength(6)]
    public string Period { get; set; }

    public bool Recalculate { get; set; } = false;
}
