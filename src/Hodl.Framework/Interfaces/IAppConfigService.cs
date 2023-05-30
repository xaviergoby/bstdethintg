namespace Hodl.Interfaces;

public interface IAppConfigService
{
    Task<T> GetAppConfigAsync<T>(string name, T defaultValue, CancellationToken cancellationToken);

    Task<IDictionary<string, T>> GetAppConfigsBeginsWith<T>(string name, T defaultValue, CancellationToken cancellationToken);

    Task<bool> SetAppConfigAsync(string name, object value, string roleName, CancellationToken cancellationToken);

    Task<bool> RequestProcessLock(string processName, int timeoutInSeconds, CancellationToken cancellationToken);

    Task<bool> WaitForProcessLock(string processName, int timeoutInMiliSeconds, CancellationToken cancellationToken);

    Task<bool> ReleaseProcessLock(string processName, CancellationToken cancellationToken);

}
