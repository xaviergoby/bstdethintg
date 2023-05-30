namespace Hodl.ExplorerAPI.Configurations;

public class EtherScanOptions
{
    public string ApiKey { get; set; }
    public int CallsPerSecond { get; set; } = 5;
    public int CallsPerDay { get; set; } = 100000;
}
