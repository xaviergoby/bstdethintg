namespace Hodl.Api.ViewModels.StatisticsModels;

public class AssetFundAggPriceStats
{
    public Guid FundId { get; set; }

    public string FundName { get; set; } = string.Empty;

    /// <summary>
    /// The total ammount of assets bought or sold in the aggregated period.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The price is the ammount of Quote asset you get for one Base asset
    /// </summary>
    public decimal Price { get => Total / Amount; }

    /// <summary>
    /// The total cost is the ammount of quote currency that is used to buy or 
    /// sell the ammount of Base assets.
    /// TotalCost = e.g. "BTC Cost", cost in quote currency
    /// </summary>
    public decimal Total { get; set; }
}
