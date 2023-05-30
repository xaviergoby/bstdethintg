namespace Hodl.Api.Interfaces;

public interface IChangeLogService
{
    Task<IEnumerable<AppChangeLog>> GetChangesAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IEnumerable<AppChangeLog>> GetChangesAsync(AppUser user, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IEnumerable<AppChangeLog>> GetChangesAsync(string tableName, DateTime from, DateTime to, CancellationToken cancellationToken = default);

    Task AddChangeLogAsync(string tableName, object oldRecord, object newRecord, CancellationToken cancellationToken = default);
}
