namespace Hodl.Api.FilterParams;

public class TransferFilterParams : IFilterParams<Transfer>
{
    [Filter(FilterType.Equals)]
    public Guid? HoldingId { get; set; }

    [Filter(FilterType.Equals)]
    public TransactionType? TransactionType { get; set; }

    [Filter(FilterType.Equals)]
    public string TransactionSource { get; set; }

    [Filter(FilterType.Equals)]
    public TransferDirection? Direction { get; set; }

    [Filter(FilterType.GreaterOrEquals)]
    public string FromBookingPeriod { get; set; }

    [Filter(FilterType.LessOrEquals)]
    public string ToBookingPeriod { get; set; }
}
