namespace Hodl.Api.ViewModels.StatisticsModels;

public class AssetPriceStats
{

    public Guid BaseAssetId { get; set; }

    public string BaseAssetName { get; set; }

    public string BaseAssetSymbol { get; set; }

    public Guid QuoteAssetId { get; set; }

    public string QuoteAssetName { get; set; }

    public string QuoteAssetSymbol { get; set; }

    public ICollection<AssetFundAggPriceStats> AssetFundAggPriceStats { get; set; } = new List<AssetFundAggPriceStats>();

    public ICollection<AssetBookingPeriodAggPriceStats> BookingPeriodAggPriceStats { get; } = new List<AssetBookingPeriodAggPriceStats>();

}
