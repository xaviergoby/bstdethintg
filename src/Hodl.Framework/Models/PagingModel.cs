namespace Hodl.Models;

public class PagingModel<TEntity>
{
    public const int MIN_PAGE_SIZE = 5;
    public const int MAX_PAGE_SIZE = 500;

    private int _page;
    private int _pageSize;
    private int _totalItems;
    private int _totalPages;

    /// <summary>
    /// CurrentPage is the page number that is selected. It is 1-based to 
    /// comply with page numbering in the user interface.
    /// </summary>
    public int CurrentPage
    {
        get => _page;
        set => _page = Math.Max(1, Math.Min(TotalPages, value));
    }

    /// <summary>
    /// The number of items shown on each page. Changing this value updates the
    /// TotalPages. The PageSize is limited to a number between 5 and 500.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set
        {
            _pageSize = Math.Max(MIN_PAGE_SIZE, Math.Min(MAX_PAGE_SIZE, value));
            CalcTotalPages();
        }
    }

    /// <summary>
    /// The total number of items in the resultset.
    /// </summary>
    public int TotalItems
    {
        get => _totalItems;
        set
        {
            _totalItems = value;
            CalcTotalPages();
        }
    }

    /// <summary>
    /// Calculated total number of pages. Read only value.
    /// </summary>
    public int TotalPages { get => _totalPages; }

    public IList<TEntity> Items { get; set; } = new List<TEntity>();

    private void CalcTotalPages()
    {
        _totalPages = (int)Math.Ceiling((decimal)_totalItems / (decimal)_pageSize);
        _page = Math.Min(TotalPages, _page);
    }
}
