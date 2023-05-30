using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.FundModels;

public class FundOwnerListViewModel
{
    [Key]
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Country { get; set; }
}
