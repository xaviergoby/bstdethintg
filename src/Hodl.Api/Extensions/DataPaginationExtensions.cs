namespace Hodl.Api.Extensions;

public static class DataPaginationExtensions
{
    /// <summary>
    /// Paginate creates a PagingModel class from the IQueryable<TEntity> and 
    /// selects the data to be returned in the resultset. The PageModel does boundary
    /// checking so the selected page is always within a valid selection, but can 
    /// differ from the requested page.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public static PagingModel<TEntity> Paginate<TEntity>(
        this IQueryable<TEntity> query,
        int page,
        int pageSize)
    {
        // TODO: Add Sort and filter options

        // Important! The limits are automatically determined in the PaginModel class
        // so first add the PageSize, TotalItems and CurrentPage to the model. Then the 
        // CurrentPage can be taken from the model to prevent out of bounds selections.
        var paged = new PagingModel<TEntity>
        {
            PageSize = pageSize,
            TotalItems = query.Count(),
            CurrentPage = page
        };

        var startRow = (paged.CurrentPage - 1) * paged.PageSize;

        paged.Items = query
            .Skip(startRow)
            .Take(paged.PageSize)
            .ToList();

        return paged;
    }

    /// <summary>
    /// PaginateAsync creates a PagingModel class from the IQueryable<TEntity> and 
    /// selects the data to be returned in the resultset. The PageModel does boundary
    /// checking so the selected page is always within a valid selection, but can 
    /// differ from the requested page.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="query"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<PagingModel<TEntity>> PaginateAsync<TEntity>(
        this IQueryable<TEntity> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // TODO: Add Sort and filter options

        // Important! The limits are automatically determined in the PaginModel class
        // so first add the PageSize, TotalItems and CurrentPage to the model. Then the 
        // CurrentPage can be taken from the model to prevent out of bounds selections.
        var paged = new PagingModel<TEntity>
        {
            PageSize = pageSize,
            TotalItems = await query.CountAsync(cancellationToken),
            CurrentPage = page
        };

        var startRow = (paged.CurrentPage - 1) * paged.PageSize;

        paged.Items = await query
            .Skip(startRow)
            .Take(paged.PageSize)
            .ToListAsync(cancellationToken);

        return paged;
    }
}
