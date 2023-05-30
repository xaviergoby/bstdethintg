namespace Hodl.Api.ViewModels.FundModels;

public class FundCategoryViewModel
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Group { get; set; }

    public byte MinPercentage { get; set; }

    public byte MaxPercentage { get; set; }
}
