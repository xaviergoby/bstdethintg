namespace Hodl.Api.Services.BackgroundServices;

public abstract class BackgroundRoutineService : BackgroundService
{
    private const string PROCESS_STARTED = "{timestamp} - {processname} started";
    private const string PROCESS_FINISHED = "{timestamp} - {processname} finished";
    private const string PROCESS_FAILED = "{timestamp} - {processname} failed:\r\n{errormessage}";
    private const int DAILY_RUN_DELAY_IN_SECONDS = 600; // Check every 10 minutes

    protected delegate Task<TimeSpan> CheckAndRunAction(IServiceScope scope, CancellationToken cancellationToken);
    protected delegate Task<bool> IntervalAction(IServiceScope scope, CancellationToken cancellationToken);

    protected readonly IServiceScopeFactory _serviceScopeFactory;
    protected readonly ILogger<BackgroundRoutineService> _logger;

    private DateTime _lastRunDate = DateTime.MinValue;

    public BackgroundRoutineService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<BackgroundRoutineService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// The loop that is run untill the application stops.
    /// </summary>
    /// <param name="checkAndRun"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task StartLoopAsync(CheckAndRunAction checkAndRun, CancellationToken cancellationToken)
    {
        // Continue to loop until the cancellationToken is used to signal that
        // the process should be canceled. The called function returns a delay
        // in milliseconds before the next run is executed.
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var delay = await checkAndRun(scope, cancellationToken);

                await Task.Delay(delay, cancellationToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                _logger.LogError(LogFormat.LOG_TIMESTAMP_MESSAGE, DateTime.Now, e.Message);
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
        }
    }

    /// <summary>
    /// Check and lock for intervalled processes in background services. 
    /// Prevents running on multiple machines or threats at teh same time.
    /// 
    /// Flow:
    /// 1. Check last update timestamp, if longer ago than interval then proceed
    /// 2. Ask for lock on process (only proceed when lock is granted)
    /// 3. Start try block
    /// 4. Do update process
    /// 5. Update last update timestamp
    /// 6. Enter finally
    /// 7. Release lock
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="interval"></param>
    /// <param name="processName"></param>
    /// <param name="intervalRoutine"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<TimeSpan> CheckAndRunIntervalled(IServiceScope scope, string processName, int interval, IntervalAction intervalRoutine, CancellationToken cancellationToken)
    {
        var appConfigService = scope.ServiceProvider.GetRequiredService<IAppConfigService>();
        var lastRunTimeStamp = await appConfigService.GetAppConfigAsync(processName + ".TimeStamp", DateTime.MinValue, cancellationToken);
        DateTime utcNow = DateTime.UtcNow;

        if (CheckTimeoutExpired(lastRunTimeStamp, utcNow, interval)
            && await appConfigService.RequestProcessLock(processName, interval, cancellationToken))
        {
            try
            {
                if (await intervalRoutine(scope, cancellationToken))
                {
                    lastRunTimeStamp = utcNow;
                    await appConfigService.SetAppConfigAsync(processName + ".TimeStamp", lastRunTimeStamp, string.Empty, cancellationToken);
                }
            }
            finally
            {
                await appConfigService.ReleaseProcessLock(processName, cancellationToken);
            }
        }

        return TimeSpan.FromSeconds(Math.Max(30, interval - DateTime.UtcNow.Subtract(lastRunTimeStamp).TotalSeconds));
    }

    /// <summary>
    /// Checks if update is needed. When the process must run at a specific
    /// time. It only runs ones in the whole system, when a lock is granted, the
    /// balances update is triggered.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<TimeSpan> CheckAndRunAtSpecificTime(IServiceScope scope, string processName, Func<DateTime> newDate, IntervalAction intervalRoutine, CancellationToken cancellationToken)
    {
        // Because we don't want to query the database all the time, we check
        // the last run date we store in the object to check for the next run.
        DateTime newRunDate = newDate();


        // Check and lock for updating the balances
        // Flow:
        // 1. Check last update, if the hour is met and date is for a new date
        // 2. Ask for lock on process (only proceed when lock is granted)
        // 3. Start try block
        // 4. Do balance update process
        // 5. Update last update timestamp
        // 6. Enter finally
        // 7. Release lock

        if (_lastRunDate < newRunDate)
        {
            // Get the latest date from the config, maybe an other node has already updated.
            var appConfigService = scope.ServiceProvider.GetRequiredService<IAppConfigService>();
            _lastRunDate = await appConfigService.GetAppConfigAsync(processName + ".TimeStamp", _lastRunDate, cancellationToken);

            if (_lastRunDate < newRunDate &&
                await appConfigService.RequestProcessLock(processName, 60, cancellationToken))
            {
                try
                {
                    _logger.LogInformation(PROCESS_STARTED, processName, DateTime.Now);

                    // Update the current balances.
                    if (await intervalRoutine(scope, cancellationToken))
                    {
                        _lastRunDate = newRunDate;
                        await appConfigService.SetAppConfigAsync(processName + ".TimeStamp", _lastRunDate, string.Empty, cancellationToken);
                    }

                    _logger.LogInformation(PROCESS_FINISHED, processName, DateTime.Now);
                }
                catch (Exception e)
                {
                    _logger.LogError(PROCESS_FAILED, processName, DateTime.Now, e.Message);
                }
                finally
                {
                    await appConfigService.ReleaseProcessLock(processName, cancellationToken);
                }
            }
        }

        return TimeSpan.FromSeconds(DAILY_RUN_DELAY_IN_SECONDS);
    }

    /// <summary>
    /// Checks if the update interval is expired. It also triggers when a new day is started.
    /// </summary>
    /// <param name="lastUpdate"></param>
    /// <param name="utcNow"></param>
    /// <returns></returns>
    protected static bool CheckTimeoutExpired(DateTime lastUpdate, DateTime utcNow, int interval)
    {
        return lastUpdate.AddSeconds(interval) <= utcNow
            || lastUpdate.DayOfYear != utcNow.DayOfYear;
    }
}
