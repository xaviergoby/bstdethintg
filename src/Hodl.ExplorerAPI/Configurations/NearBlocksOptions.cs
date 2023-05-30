namespace Hodl.ExplorerAPI.Configurations;

public class NearBlocksOptions
{
    public string ApiKey { get; set; }

    public int CallsPerMinute { get; set; } = 6;

    public int CallsPerDay { get; set; } = 333;

    public int CallsPerMonth { get; set; } = 10000;
}
