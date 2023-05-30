namespace Hodl.MarketAPI.Models;

public class MarketListing
{
    public string Symbol { get; set; }

    public string Source { get; set; }

    public DateTime TimeStamp { get; set; }

    public int CmcRank { get; set; }

    public decimal CirculatingSupply { get; set; }

    public decimal TotalSupply { get; set; }

    public decimal MaxSupply { get; set; }

    public decimal USDPrice { get; set; }

    public decimal BTCPrice { get; set; }

    public decimal Volume_24h { get; set; }

    public decimal PercentChange_1h { get; set; }

    public decimal PercentChange_24h { get; set; }

    public decimal PercentChange_7d { get; set; }

    public decimal MarketCap { get; set; }
}
