using System.Text.Json;

namespace Hodl.Api.Services;

public class AppConfigService : IAppConfigService
{
    private const string TypeDeserializeErrorMessage = "Error reading Appconfig value {name} into expected class {type}";

    private readonly HodlDbContext _db;
    private readonly ILogger<AppConfigService> _logger;

    public AppConfigService(
        HodlDbContext dbContext,
        ILogger<AppConfigService> logger)
    {
        _db = dbContext;
        _logger = logger;
    }

    public async Task<T> GetAppConfigAsync<T>(string name, T defaultValue, CancellationToken cancellationToken)
    {
        var appConfig = await _db.AppConfigs
            .Where(s => s.Name == name)
            .FirstOrDefaultAsync(cancellationToken);

        if (appConfig != null && !string.IsNullOrEmpty(appConfig.Value))
            try
            {
                return JsonSerializer.Deserialize<T>(appConfig.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, TypeDeserializeErrorMessage, name, typeof(T).Name);
                // And reset the value to prevent further errors
                _db.AppConfigs.Remove(appConfig);
                await _db.SaveChangesAsync(cancellationToken);
            }

        return defaultValue;
    }

    public async Task<IDictionary<string, T>> GetAppConfigsBeginsWith<T>(string name, T defaultValue, CancellationToken cancellationToken)
    {
        var appConfigs = await _db.AppConfigs
            .Where(s => s.Name.StartsWith(name))
            .ToArrayAsync(cancellationToken);

        var result = new Dictionary<string, T>();

        foreach (var appConfig in appConfigs)
        {
            if (string.IsNullOrEmpty(appConfig.Value))
            {
                result[appConfig.Name] = defaultValue;
            }
            else
            {
                try
                {
                    result[appConfig.Name] = JsonSerializer.Deserialize<T>(appConfig.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, TypeDeserializeErrorMessage, typeof(T).Name);
                }
            }
        }

        return result;
    }

    public async Task<bool> SetAppConfigAsync(string name, object value, string roleName, CancellationToken cancellationToken)
    {
        AppConfig storedAppConfig = await _db.AppConfigs.FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
        AppConfig newAppConfig = new()
        {
            Name = name,
            Value = value == null ? string.Empty : JsonSerializer.Serialize(value),
            DateTime = DateTime.UtcNow,
            NormalizedRoleName = roleName?.ToUpperInvariant(),
        };

        if (storedAppConfig == null)
        {
            await _db.AppConfigs.AddAsync(newAppConfig, cancellationToken);
        }
        else
        {
            storedAppConfig.Value = newAppConfig.Value;
            storedAppConfig.DateTime = newAppConfig.DateTime;
            storedAppConfig.NormalizedRoleName = newAppConfig.NormalizedRoleName;

            _db.AppConfigs.Update(storedAppConfig);
        }

        return await _db.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> RequestProcessLock(string processName, int timeoutInSeconds, CancellationToken cancellationToken)
    {
        var lockRecord = await GetAppConfigAsync<ProcessLockModel>(processName, null, cancellationToken);

        if (NoLockOrTimedOut(lockRecord))
        {
            ProcessLockModel newLockRecord = new()
            {
                AppMutex = AppMutex.Mutex,
                LockTimeStamp = DateTime.UtcNow,
                LockTimeout = timeoutInSeconds
            };
            return await SetAppConfigAsync(processName, newLockRecord, string.Empty, cancellationToken);
        }

        return false;
    }

    public async Task<bool> ReleaseProcessLock(string processName, CancellationToken cancellationToken)
    {
        var lockRecord = await GetAppConfigAsync<ProcessLockModel>(processName, null, cancellationToken);

        if (lockRecord?.AppMutex == AppMutex.Mutex)
        {
            return await SetAppConfigAsync(processName, null, string.Empty, cancellationToken);
        }

        return lockRecord == null;
    }

    private static bool NoLockOrTimedOut(ProcessLockModel lockRecord)
    {
        return lockRecord == null ||
            lockRecord.AppMutex == null ||
            lockRecord.AppMutex == AppMutex.Mutex ||
            (lockRecord.LockTimeout > 0 &&
            DateTime.UtcNow > lockRecord.LockTimeStamp.AddSeconds(lockRecord.LockTimeout));
    }

    public async Task<bool> WaitForProcessLock(string processName, int timeoutInMiliSeconds, CancellationToken cancellationToken)
    {
        DateTime timeout = DateTime.Now.AddMilliseconds(timeoutInMiliSeconds);

        while (DateTime.Now <= timeout)
        {
            if (await RequestProcessLock(processName, timeoutInMiliSeconds / 1000, cancellationToken))
                return true;

            // Wait a little
            await Task.Delay(timeoutInMiliSeconds / 6, cancellationToken);
        }

        return false;
    }
}
