using AutoMapper.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Hodl.Api.Services;

public class ChangeLogService : IChangeLogService
{
    /// <summary>
    /// The LoggingPropertiesResolver filters the referenced properties from 
    /// the data model. This is done to be able to log only a single layer of 
    /// values, only the values that are actually stored in the database, and
    /// not the referenced values.
    /// 
    /// NOTE: Untill there is a good solution in System.Text.Json, we are 
    /// obligated to use Newtonsoft.Json for this filter method.
    /// </summary>
    private class LoggingPropertiesResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var allProps = base.CreateProperties(type, memberSerialization);

            return allProps.Where(p => !IsReferencedProperty(p)).ToList();
        }

        private static bool IsReferencedProperty(JsonProperty prop)
        {
            return
                prop.PropertyType.Namespace.StartsWith("Hodl.Api.HodlDbDomain") ||
                prop.PropertyType.IsCollection();
        }
    }


    private readonly HodlDbContext _db;
    private readonly IUserResolver _userResolver;

    private readonly JsonSerializerSettings _settings = new()
    {
        ContractResolver = new LoggingPropertiesResolver()
    };

    public ChangeLogService(
        HodlDbContext dbContext,
        IUserResolver userResolver)
    {
        _db = dbContext;
        _userResolver = userResolver;
    }

    public async Task AddChangeLogAsync(string tableName, object oldRecord, object newRecord, CancellationToken cancellationToken)
    {
        var user = await _userResolver.GetUser();
        var oldRecordJson = JsonConvert.SerializeObject(oldRecord, _settings);
        var newRecordJson = JsonConvert.SerializeObject(newRecord, _settings);

        AppChangeLog log = new()
        {
            DateTime = DateTime.Now.ToUniversalTime(),
            UserId = user?.Id,
            NormalizedUserName = user?.NormalizedUserName ?? string.Empty,
            NormalizedRoleName = user?.Roles.FirstOrDefault()?.ToUpperInvariant() ?? string.Empty,
            TableName = tableName,
            OldRecord = oldRecordJson,
            NewRecord = newRecordJson
        };
        await _db.AppChanges.AddAsync(log, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<AppChangeLog>> GetChangesAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _db.AppChanges
            .Where(c => c.DateTime >= from && c.DateTime <= to)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AppChangeLog>> GetChangesAsync(AppUser user, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _db.AppChanges
            .Where(c => c.UserId == user.Id && c.DateTime >= from && c.DateTime <= to)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AppChangeLog>> GetChangesAsync(string tableName, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _db.AppChanges
            .Where(c => c.TableName == tableName && c.DateTime >= from && c.DateTime <= to)
            .ToListAsync(cancellationToken);
    }
}
