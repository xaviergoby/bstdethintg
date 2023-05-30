using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.FundModels;

public class FundOwnerEditViewModel
{
    [Required, MaxLength(60)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [MaxLength(60)]
    public string Department { get; set; } = string.Empty;

    [MaxLength(60)]
    public string Country { get; set; }
}
