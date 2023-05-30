namespace Hodl.Api.ViewModels.FundModels;

public class FundOwnerDetailViewModel
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Country { get; set; }

    public virtual ICollection<FundListViewModel> Funds { get; set; }
}
