namespace Hodl.Api.FilterParams;

public class CategoryFilterParams : IFilterParams<Category>
{
    [Filter(FilterType.Equals)]
    public string Group { get; set; }
}
