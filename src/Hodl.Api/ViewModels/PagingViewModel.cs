namespace Hodl.Api.ViewModels;

public record PagingViewModel<TItemViewModel>
{
    public int CurrentPage { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }

    public IList<TItemViewModel> Items { get; set; }
}
