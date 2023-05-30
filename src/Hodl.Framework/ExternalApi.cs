using Hodl.Constants;
using Hodl.Extensions;
using Hodl.Framework.Utils;
using Hodl.Interfaces;
using Hodl.Models;
using Hodl.Utils;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Hodl.Framework;

public abstract class ExternalApi
{
    protected readonly IAppConfigService _appConfigService;
    protected readonly INotificationManager _notificationManager;
    protected readonly ILogger<ExternalApi> _logger;

    private ExternalApiStateModel apiState = null;
    private int waitTime = 500; // in milliseconds

    protected abstract string ApiStateConfigName { get; }
    protected abstract string ApiMessageTitle { get; }
    protected abstract string ApiMessageContent { get; }

    public ExternalApi(
        IAppConfigService appConfigService,
        INotificationManager notificationManager,
        ILogger<ExternalApi> logger
        )
    {
        _appConfigService = appConfigService;
        _notificationManager = notificationManager;
        _logger = logger;
    }

    internal async Task<ExternalApiStateModel> GetApiState(CancellationToken cancellationToken)
    {
        apiState ??= await _appConfigService.GetAppConfigAsync(ApiStateConfigName,
            new ExternalApiStateModel
            {
                AppMutex = AppMutex.Mutex,
                TimeStamp = DateTime.MinValue,
                State = ExternalApiState.Unknown,
                StatusCode = -1,
                ErrorMessage = string.Empty
            },
            cancellationToken);

        return apiState;
    }

    internal async Task SetApiOnline(CancellationToken cancellationToken)
    {
        // Reset wait time
        waitTime = 500;

        var apiState = await GetApiState(cancellationToken);

        if (apiState.State != ExternalApiState.Online)
        {
            lock (this.apiState)
            {
                apiState.AppMutex = AppMutex.Mutex;
                apiState.TimeStamp = DateTime.UtcNow;
                apiState.State = ExternalApiState.Online;
                apiState.StatusCode = 200;
                apiState.ErrorMessage = string.Empty;
            }
            await _appConfigService.SetAppConfigAsync(ApiStateConfigName, apiState, string.Empty, cancellationToken);
        }
    }

    internal async Task SetApiError(HttpRequestException ex, CancellationToken cancellationToken)
    {
        var apiState = await GetApiState(cancellationToken);
        var errApiState = ex.GetApiState();

        if (apiState.State != errApiState.State)
        {
            lock (this.apiState)
            {
                this.apiState = errApiState;
            }
            await _appConfigService.SetAppConfigAsync(ApiStateConfigName, errApiState, string.Empty, cancellationToken);
            await _notificationManager.SendNotification(
                source: StackTraceHelper.CallingMethod(this),
                type: NotificationType.Error,
                title: ApiMessageTitle,
                message: string.Format(ApiMessageContent, ex.Message),
                info: string.Format(LogFormat.FORMAT_TIMESTAMP_MESSAGE, DateTime.Now, ex.Message),
                role: UserRoles.Admin,
                cancellationToken: cancellationToken);
        }
    }

    public async Task<T> ApiRequestAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        T result = default;
        while (true)
            try
            {
                result = await action();

                if (result is HttpResponseMessage r &&
                    r.StatusCode.Equals(HttpStatusCode.TooManyRequests) &&
                    Wait())
                    continue;

                // Set the API state to online
                await SetApiOnline(cancellationToken);

                return result;
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode.Equals(HttpStatusCode.TooManyRequests) && Wait())
                    continue;

                _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, DateTime.Now, ex.Message);
                // Save the API state and send the message when the state has changed
                await SetApiError(ex, cancellationToken);
                return result;
            }
    }

    /// <summary>
    /// Increasing sleep timer to retry after TooManyRequests. When we waited 
    /// the result is true. If timer is too long, return false and don't wait
    /// anymore.
    /// </summary>
    /// <returns></returns>
    private bool Wait()
    {
        waitTime *= 2;
        if (waitTime > 64000) return false;

        Thread.Sleep(waitTime);
        return true;
    }
}
