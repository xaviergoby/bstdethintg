namespace Hodl.Api.FilterParams;

public class OrderFilterParams : IFilterParams<Order>
{
    [Filter(FilterType.Equals)]
    public string WalletAddres { get; set; }

    [Filter(FilterType.Equals)]
    public OrderDirection? Direction { get; set; }

    [Filter(FilterType.Equals)]
    public OrderState? State { get; set; }

    [Filter(FilterType.GreaterOrEquals)]
    public DateTime? FromDateTime { get; set; }

    [Filter(FilterType.LessOrEquals)]
    public DateTime? ToDateTime { get; set; }

    [Filter(FilterType.Contains)]
    public string OrderNumber { get; set; }

    [Filter(FilterType.InCollection)]
    public ICollection<Guid> BaseAssetId { get; set; }

    [Filter(FilterType.InCollection)]
    public ICollection<Guid> QuoteAssetId { get; set; }

    [Filter(FilterType.InCollection)]
    public ICollection<Guid> Id { get; set; }
}
