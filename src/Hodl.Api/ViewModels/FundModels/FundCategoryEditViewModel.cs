namespace Hodl.Api.ViewModels.FundModels;

public class FundCategoryEditViewModel
{
    public Guid CategoryId { get; set; }

    public byte MinPercentage { get; set; }

    public byte MaxPercentage { get; set; }
}
