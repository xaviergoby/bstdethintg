namespace Hodl.Api.FilterParams;

public class WalletFilterParams : IFilterParams<Wallet>
{
    [Filter(FilterType.Equals)]
    public Guid? FundId { get; set; }

    [Filter(FilterType.Equals)]
    public Guid? ExchangeAccountId { get; set; }

}
