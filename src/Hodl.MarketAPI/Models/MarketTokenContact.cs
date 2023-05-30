namespace Hodl.MarketAPI.Models;

public class MarketTokenContact
{
    public string Symbol { get; set; }

    public string ContractAddress { get; set; }

    public string Network { get; set; }

    public string NetworkToken { get; set; }

    public long ChainId { get; set; }
}
