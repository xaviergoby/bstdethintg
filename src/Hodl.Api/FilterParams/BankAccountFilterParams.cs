namespace Hodl.Api.FilterParams;

public class BankAccountFilterParams : IFilterParams<BankAccount>
{
    [Filter(FilterType.Equals)]
    public Guid? FundId { get; set; }
}
