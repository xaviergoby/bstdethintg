namespace Hodl.Api.FilterParams;

public class CryptoFilterParams : IFilterParams<CryptoCurrency>
{
    [Filter(FilterType.StartsWith)]
    public string Symbol { get; set; }

    [Filter(FilterType.Contains)]
    public string Name { get; set; }
}
