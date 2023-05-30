namespace Hodl.Api.ViewModels.FundModels;

public class FundLayerViewModel
{
    public byte LayerIndex { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public byte AimPercentage { get; set; }

    public byte AlertRangeLow { get; set; }

    public byte AlertRangeHigh { get; set; }
}
