using Hodl.Api.Services.Notifications.NotificationModels;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Hodl.Api.Services.BackgroundServices;

/// <summary>
/// This background service runs periodically to retrieve real world currency rates.
/// </summary>
public class FundBackgroundService : BackgroundRoutineService
{
    private const int DAILY_NAV_DELAY_IN_SECONDS = 60;
    private const int CLOSE_BOOKING_PERIOD_DELAY_IN_SECONDS = 300;
    private const string DAILY_NAV_PROCESSNAME = "DailyNav.Calculating.Task";
    private const string CLOSE_BOOKING_PERIOD_PROCESSNAME = "BookingPeriod.Closing.Task";
    private const string FUND_LAYER_BOUNDARIES_CHECK_PROCESSNAME = "Fund.Layers.Boundaries.Check";
    private const string FUND_CATEGORIES_BOUNDARIES_CHECK_PROCESSNAME = "Fund.Categories.Boundaries.Check";

    // LogInfo are used for logging, infoMessages for notifications to the end user
    private const string LOG_INFO_CLOSING_BOOKING_PERIOD = "{timestamp} - Close booking period {period} for fundid {fund}";
    private const string LOG_INFO_CLOSED_BOOKING_PERIOD = "{timestamp} - Booking period {period} for fundid {fund} is closed. Booking period {newperiod} is now active.";
    private const string LOG_INFO_FAILED_BOOKING_PERIOD = "{timestamp} - Close booking period {period} for fundid {fund} failed. Errro message:\r\n{message}";
    private const string MESSAGE_CLOSED_BOOKING_PERIOD = "{0} - Booking period {1} for fundid {2} is closed. Booking period {3} is now active.";
    private const string INFO_CLOSED_BOOKING_PERIOD = "{0} - Booking period {1} for fundid {2} is closed. Booking period {3} is now active.";
    private const string MESSAGE_FAILED_BOOKING_PERIOD = "{0} - Close booking period {1} for fundid {2} failed.";
    private const string INFO_FAILED_BOOKING_PERIOD = "{0} - Close booking period {1} for fundid {2} failed. Errro message:\r\n{3}";

    private const string LOG_INFO_FUND_LAYER_BOUNDARIES_ALERT = "{timestamp} - A fund layer boundary is breached.";
    private const string LOG_INFO_FUND_LAYER_BOUNDARIES_ALERT_RESTORE = "{timestamp} - Fund layer boundaries are restored.";
    private const string INFO_MESSAGE_FUND_LAYER_BOUNDARIES_ALERT = "A fund layer boundary is breached.";

    private const string LOG_INFO_FUND_CATEGORY_BOUNDARIES_ALERT = "{timestamp} - A fund category boundary is breached.";
    private const string LOG_INFO_FUND_CATEGORY_BOUNDARIES_ALERT_RESTORE = "{timestamp} - Fund categorie boundaries are restored.";
    private const string INFO_MESSAGE_FUND_CATEGORY_BOUNDARIES_ALERT = "A fund category boundary is breached.";

    private const string LOG_INFO_CREATE_DAILY_NAV = "{timestamp} - Create daily NAV of {date:d} for fundid {fund}";
    private const string LOG_INFO_DAILY_NAV_CREATED = "{timestamp} - Daily NAV of {date:d} for fundid {fund} is created.";
    private const string LOG_INFO_DAILY_NAV_FAILED = "{timestamp} - Daily NAV of {date:d} for fundid {fund} failed. Errro message:\r\n{message}";
    private const string MESSAGE_DAILY_NAV_CREATED = "{0} - Daily NAV of {1:d} for fundid {2} is created.";
    private const string INFO_DAILY_NAV_CREATED = "{0} - Daily NAV of {1:d} for fundid {2} is created.";
    private const string MESSAGE_DAILY_NAV_FAILED = "{0} - Daily NAV of {1:d} for fundid {2} failed.";
    private const string INFO_DAILY_NAV_FAILED = "{0} - Daily NAV of {1:d} for fundid {2} failed. Errro message:\r\n{3}";

    private const string EMAIL_INFO_MESSAGE_SUBJECT = "Fund background service INFO message";
    private const string EMAIL_ERROR_MESSAGE_SUBJECT = "Fund background service ERROR message";


    private readonly IBookingPeriodHelper _bookingPeriodHelper;

    private DateTime _lastNavDate = DateTime.MinValue;
    private string _activeBookingPeriod = "000000";
    private readonly int _fundLayerBoundariesCheckInterval;
    private readonly int _fundCategoryBoundariesCheckInterval;


    public FundBackgroundService(
        IBookingPeriodHelper bookingPeriodHelper,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<AppDefaults> settings,
        ILogger<FundBackgroundService> logger)
        : base(serviceScopeFactory, logger)
    {
        _bookingPeriodHelper = bookingPeriodHelper;
        _fundLayerBoundariesCheckInterval = settings.Value.FundLayersCheckIntervalInSeconds;
        _fundCategoryBoundariesCheckInterval = settings.Value.FundLayersCheckIntervalInSeconds;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            StartLoopAsync(DailyNavCheckAndRun, cancellationToken),
            StartLoopAsync(CloseBookingPeriodCheckAndRun, cancellationToken),
            StartLoopAsync(CheckLayerRangesCheckAndRun, cancellationToken),
            StartLoopAsync(CheckCategoryRangesCheckAndRun, cancellationToken)
            );
    }

    protected async Task<TimeSpan> DailyNavCheckAndRun(IServiceScope scope, CancellationToken cancellationToken)
    {
        // Because we don't want to query the database all the time, we check
        // the last run date we store in the object to check for the next run.
        DateTime newNavDate = _bookingPeriodHelper.NavDate(DateTimeOffset.UtcNow);

        if (_lastNavDate < newNavDate)
        {
            // Check and lock for creating the daily NAV
            // Flow:
            // 1. Check last update DailyNAV, if the hour is met and timestamp
            // 2. Ask for lock on process (only proceed when lock is granted)
            // 3. Start try block
            // 4. Do calculation process
            // 5. Update last update timestamp
            // 6. Enter finally
            // 7. Release lock

            var appConfigService = scope.ServiceProvider.GetRequiredService<IAppConfigService>();

            // Get the latest date from the config, maybe an other node has already updated.
            _lastNavDate = await appConfigService.GetAppConfigAsync(AppConfigs.DAILY_NAV_TIMESTAMP, DateTime.MinValue, cancellationToken);

            // Because another process or host can run the same process we have to check the last
            // update timestamp. If we can run the process because the interval expired, we try to 
            // get a lock on the processname. This is to ensure only one service will execute the 
            // update process, not all running services at the same time.
            if (_lastNavDate < newNavDate &&
                await appConfigService.RequestProcessLock(DAILY_NAV_PROCESSNAME, 60 * 60, cancellationToken))
            {
                try
                {
                    // Create from the last created Daily NAV until the NAV of today.
                    DateTime calcNavDate = _lastNavDate.AddDays(1);

                    while (calcNavDate <= newNavDate)
                    {
                        if (await CalculateDailyNavs(scope, calcNavDate, cancellationToken))
                        {
                            await appConfigService.SetAppConfigAsync(AppConfigs.DAILY_NAV_TIMESTAMP, calcNavDate, string.Empty, cancellationToken);
                            calcNavDate = calcNavDate.AddDays(1);
                        }
                    }
                    _lastNavDate = newNavDate;
                }
                finally
                {
                    await appConfigService.ReleaseProcessLock(DAILY_NAV_PROCESSNAME, cancellationToken);
                }
            }
        }

        return TimeSpan.FromSeconds(DAILY_NAV_DELAY_IN_SECONDS);
    }

    protected async Task<TimeSpan> CloseBookingPeriodCheckAndRun(IServiceScope scope, CancellationToken cancellationToken)
    {
        // Because we don't want to create a scope and database connections all the time,
        // we check the last run date we store in the object to check for the next run.
        string currentBookingPeriod = _bookingPeriodHelper.CalcBookingPeriod(DateTimeOffset.UtcNow);

        if (_activeBookingPeriod.CompareTo(currentBookingPeriod) < 0)
        {
            // Check and lock for creating the daily NAV
            // Flow:
            // 1. Check last booking period, if new period has started close the
            //    last period and create the holdings for the new period.
            // 2. Ask for lock on process (only proceed when lock is granted)
            // 3. Start try block
            // 4. Do calculation process
            // 5. Update last update timestamp
            // 6. Enter finally
            // 7. Release lock

            var appConfigService = scope.ServiceProvider.GetRequiredService<IAppConfigService>();


            _activeBookingPeriod = await appConfigService.GetAppConfigAsync(AppConfigs.ACTIVE_BOOKING_PERIOD,
                _bookingPeriodHelper.GetPreviousBookingPeriod(currentBookingPeriod), cancellationToken);

            // Because another process or host can run the same process we have to check the last
            // update timestamp. If we can run the process because the interval expired, we try to 
            // get a lock on the processname. This is to ensure only one service will execute the 
            // update process, not all running services at the same time.
            if (_activeBookingPeriod.CompareTo(currentBookingPeriod) < 0 &&
                await appConfigService.RequestProcessLock(CLOSE_BOOKING_PERIOD_PROCESSNAME, 5 * 60, cancellationToken))
            {
                try
                {
                    if (await CloseBookingPeriod(scope, _activeBookingPeriod, cancellationToken))
                    {
                        _activeBookingPeriod = currentBookingPeriod;
                        await appConfigService.SetAppConfigAsync(AppConfigs.ACTIVE_BOOKING_PERIOD, _activeBookingPeriod, string.Empty, cancellationToken);
                    }
                }
                finally
                {
                    await appConfigService.ReleaseProcessLock(CLOSE_BOOKING_PERIOD_PROCESSNAME, cancellationToken);
                }
            }
        }

        return TimeSpan.FromSeconds(CLOSE_BOOKING_PERIOD_DELAY_IN_SECONDS);
    }

    protected async Task<TimeSpan> CheckLayerRangesCheckAndRun(IServiceScope scope, CancellationToken cancellationToken) =>
        await CheckAndRunIntervalled(scope,
            FUND_LAYER_BOUNDARIES_CHECK_PROCESSNAME,
            _fundLayerBoundariesCheckInterval,
            FundLayerBoundariesCheck,
            cancellationToken);

    protected async Task<TimeSpan> CheckCategoryRangesCheckAndRun(IServiceScope scope, CancellationToken cancellationToken) =>
        await CheckAndRunIntervalled(scope,
            FUND_CATEGORIES_BOUNDARIES_CHECK_PROCESSNAME,
            _fundCategoryBoundariesCheckInterval,
            FundCategoryBoundariesCheck,
            cancellationToken);

    private async Task<bool> CalculateDailyNavs(IServiceScope scope, DateTime navDate, CancellationToken cancellationToken)
    {
        // First prepare the scope dependencies
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailUserGroupService>();
        var fundService = scope.ServiceProvider.GetRequiredService<IFundService>();
        var notificationManager = scope.ServiceProvider.GetRequiredService<INotificationManager>();

        var funds = await fundService.GetActiveFunds(cancellationToken);
        int errorCounter = 0;
        var currentBookingPeriod = _bookingPeriodHelper.CalcBookingPeriod(_bookingPeriodHelper.DailyNavEndDateTime(navDate));

        foreach (Fund fund in funds)
        {
            // Is the fund active in the current booking period?
            if (fund.DateEnd != null && fund.DateEnd < navDate)
                continue;

            var fundBookingPeriod = await fundService.CurrentBookingPeriod(fund.Id, cancellationToken);
            if (string.Compare(fundBookingPeriod, currentBookingPeriod, StringComparison.OrdinalIgnoreCase) != 0)
                continue;

            _logger.LogInformation(LOG_INFO_CREATE_DAILY_NAV, DateTime.Now, navDate, fund.FundName);
            try
            {
                _ = await fundService.CreateDailyNAV(fund.Id, navDate, cancellationToken);

                emailService.ResetErrorFor("DailyNav", fund.Id.ToString());

                _logger.LogInformation(LOG_INFO_DAILY_NAV_CREATED, DateTime.Now, navDate, fund.FundName);

                await notificationManager.SendNotification(
                    source: StackTraceHelper.CallingMethod(this),
                    type: NotificationType.Information,
                    title: EMAIL_INFO_MESSAGE_SUBJECT,
                    message: string.Format(MESSAGE_DAILY_NAV_CREATED, DateTime.Now, navDate, fund.FundName),
                    info: string.Format(INFO_DAILY_NAV_CREATED, DateTime.Now, navDate, fund.FundName),
                    role: UserRoles.Admin,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                errorCounter++;
                _logger.LogError(LOG_INFO_DAILY_NAV_FAILED, DateTime.Now, navDate, fund.FundName, ex.Message);
                if (!emailService.IsRegisteredAsError("DailyNav", fund.Id.ToString()))
                {
                    emailService.RegisterErrorFor("DailyNav", fund.Id.ToString());

                    await notificationManager.SendNotification(
                        source: StackTraceHelper.CallingMethod(this),
                        type: NotificationType.Error,
                        title: EMAIL_ERROR_MESSAGE_SUBJECT,
                        message: string.Format(MESSAGE_DAILY_NAV_FAILED, DateTime.Now, navDate, fund.FundName),
                        info: string.Format(INFO_DAILY_NAV_FAILED, DateTime.Now, navDate, fund.FundName, ex.Message),
                        role: UserRoles.Admin,
                        cancellationToken: cancellationToken);
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }

        return errorCounter == 0;
    }

    private async Task<bool> CloseBookingPeriod(IServiceScope scope, string bookingPeriod, CancellationToken cancellationToken)
    {
        // The procedure will close the current booking periods for each fund
        // and make follow up-holdings for the new period. There can only be
        // one booking period active at a time. When a new fund is created and 
        // history data is still in progress, the booking periods will not be
        // closed automatically. This must be done by hand.

        // First prepare the scope dependencies
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailUserGroupService>();
        var fundService = scope.ServiceProvider.GetRequiredService<IFundService>();
        var notificationManager = scope.ServiceProvider.GetRequiredService<INotificationManager>();

        var funds = await fundService.GetActiveFundsSortedByDependency(cancellationToken);
        int errorCounter = 0;

        foreach (Fund fund in funds)
        {
            var currentPeriod = await fundService.CurrentBookingPeriod(fund.Id, cancellationToken);

            if (string.Compare(currentPeriod, bookingPeriod) == 0 &&
                (fund.DateEnd == null || fund.DateEnd > _bookingPeriodHelper.GetPeriodStartDateTime(bookingPeriod).UtcDateTime))
            {
                _logger.LogInformation(LOG_INFO_CLOSING_BOOKING_PERIOD, DateTime.Now, currentPeriod, fund.FundName);
                try
                {
                    var newPeriod = await fundService.CloseBookingPeriod(fund.Id, currentPeriod, cancellationToken: cancellationToken);

                    emailService.ResetErrorFor("CloseBookingPeriod", fund.Id.ToString());

                    _logger.LogInformation(LOG_INFO_CLOSED_BOOKING_PERIOD, DateTime.Now, currentPeriod, fund.FundName, newPeriod);

                    await notificationManager.SendNotification(
                        source: StackTraceHelper.CallingMethod(this),
                        type: NotificationType.Information,
                        title: EMAIL_INFO_MESSAGE_SUBJECT,
                        message: string.Format(MESSAGE_CLOSED_BOOKING_PERIOD, DateTime.Now, currentPeriod, fund.FundName, newPeriod),
                        info: string.Format(INFO_CLOSED_BOOKING_PERIOD, DateTime.Now, currentPeriod, fund.FundName, newPeriod),
                        role: UserRoles.Admin,
                        cancellationToken: cancellationToken);

                }
                catch (Exception ex)
                {
                    errorCounter++;

                    _logger.LogError(LOG_INFO_FAILED_BOOKING_PERIOD, DateTime.Now, currentPeriod, fund.FundName, ex.Message);

                    if (!emailService.IsRegisteredAsError("CloseBookingPeriod", fund.Id.ToString()))
                    {
                        emailService.RegisterErrorFor("CloseBookingPeriod", fund.Id.ToString());

                        await notificationManager.SendNotification(
                            source: StackTraceHelper.CallingMethod(this),
                            type: NotificationType.Error,
                            title: EMAIL_ERROR_MESSAGE_SUBJECT,
                            message: string.Format(MESSAGE_FAILED_BOOKING_PERIOD, DateTime.Now, currentPeriod, fund.FundName),
                            info: string.Format(INFO_FAILED_BOOKING_PERIOD, DateTime.Now, currentPeriod, fund.FundName, ex.Message),
                            role: UserRoles.Admin,
                            cancellationToken: cancellationToken);

                    }
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }

        return errorCounter == 0;
    }

    #region FundBoundaryBreachesChecking

    /// <summary>
    /// Check all fundlayers for boundary overrides. This method will collect 
    /// the current state of breaches and sends an alert when one or more 
    /// breaches have appeared.
    /// The state and alert state are saved in the AppSettings table.
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<bool> FundLayerBoundariesCheck(IServiceScope scope, CancellationToken cancellationToken)
    {
        var appConfigService = scope.ServiceProvider.GetRequiredService<IAppConfigService>();
        var notificationManager = scope.ServiceProvider.GetRequiredService<INotificationManager>();

        var currentAlertState = await appConfigService.GetAppConfigAsync(AppConfigs.FUND_LAYER_BOUNDARIES_ALERT_STATE, string.Empty, cancellationToken);
        var storedBreaches = await appConfigService.GetAppConfigAsync(
                AppConfigs.FUND_LAYER_BOUNDARIES_STATE,
                new List<FundBoundaryBreach>(),
                cancellationToken);
        var currentBreaches = await GenerateFundLayerBreaches(scope, cancellationToken);

        // Check if the alert state is switched ON AND if there are no existing breach alerts
        if (!string.IsNullOrEmpty(currentAlertState) && currentBreaches.Count == 0)
        {
            // The alert state must be switched OFF
            _logger.LogInformation(LOG_INFO_FUND_LAYER_BOUNDARIES_ALERT_RESTORE, DateTime.Now);
            // " Delete" notification
            if (await notificationManager.DeleteNotification(new string[] { UserRoles.Admin }, Guid.Parse(currentAlertState), cancellationToken))
            {
                _ = await appConfigService.SetAppConfigAsync(AppConfigs.FUND_LAYER_BOUNDARIES_STATE, null, string.Empty, cancellationToken);
                _ = await appConfigService.SetAppConfigAsync(AppConfigs.FUND_LAYER_BOUNDARIES_ALERT_STATE, string.Empty, string.Empty, cancellationToken);
            }
        }
        // CHeck if there are more or less currently breached fund categories than there are stored in the db.
        else if (currentBreaches.Count > 0)
        {
            // Merge the current state and update in the appsettings if the state is changed
            if (MergeAlertStates(storedBreaches, currentBreaches))
            {
                _ = await appConfigService.SetAppConfigAsync(AppConfigs.FUND_LAYER_BOUNDARIES_STATE, currentBreaches, string.Empty, cancellationToken);

                if (string.IsNullOrEmpty(currentAlertState))
                {
                    _logger.LogWarning(LOG_INFO_FUND_LAYER_BOUNDARIES_ALERT, DateTime.Now);

                    // Notification sending
                    var notificationId = await notificationManager.SendNotification(
                        source: StackTraceHelper.CallingMethod(this),
                        type: NotificationType.Warning,
                        title: FUND_LAYER_BOUNDARIES_CHECK_PROCESSNAME,
                        message: INFO_MESSAGE_FUND_LAYER_BOUNDARIES_ALERT,
                        info: JsonSerializer.Serialize(currentBreaches),
                        role: UserRoles.Admin,
                        cancellationToken: cancellationToken);

                    // Update alert state
                    _ = await appConfigService.SetAppConfigAsync(AppConfigs.FUND_LAYER_BOUNDARIES_ALERT_STATE, notificationId.ToString(), string.Empty, cancellationToken);
                }
                else
                {
                    await notificationManager.UpdateNotification(
                        new string[] { UserRoles.Admin },
                        Guid.Parse(currentAlertState),
                        type: NotificationType.Warning,
                        title: FUND_LAYER_BOUNDARIES_CHECK_PROCESSNAME,
                        message: INFO_MESSAGE_FUND_LAYER_BOUNDARIES_ALERT,
                        info: JsonSerializer.Serialize(currentBreaches),
                        role: UserRoles.Admin,
                        cancellationToken);
                }
            }
        }

        return true;
    }

    private static async Task<List<FundBoundaryBreach>> GenerateFundLayerBreaches(IServiceScope scope, CancellationToken cancellationToken = default)
    {
        // Result list
        List<FundBoundaryBreach> fundLayerBoundaryBreaches = new();

        var fundService = scope.ServiceProvider.GetRequiredService<IFundService>();

        foreach (Fund fund in await fundService.GetFundCards(cancellationToken))
        {
            var fundHoldings = await fundService.GetCurrentFundHoldings(fund.Id, true, true, cancellationToken);
            var layerDistributions = await fundService.CalcLayerDistribution(fundHoldings, cancellationToken);

            foreach (var layerDistribution in layerDistributions)
            {
                // Find the corresponding layer
                var fundLayerSettings = fund.Layers.Where(fl => fl.LayerIndex == layerDistribution.Key).SingleOrDefault();

                // No layer no breach
                if (fundLayerSettings == null)
                    continue;

                string breachSide = string.Empty; // Empty means no breach
                int breachLevel = 0;
                int currentPercentage = (int)Math.Round(layerDistribution.Value.TotalSharePercentage);

                // Now check the boundaries, upper and lower
                if (currentPercentage < fundLayerSettings.AimPercentage - fundLayerSettings.AlertRangeLow)
                {
                    breachSide = "LOWER";
                    breachLevel = fundLayerSettings.AimPercentage - fundLayerSettings.AlertRangeLow;
                }
                else if (currentPercentage > fundLayerSettings.AimPercentage + fundLayerSettings.AlertRangeHigh)
                {
                    breachSide = "UPPER";
                    breachLevel = fundLayerSettings.AimPercentage + fundLayerSettings.AlertRangeHigh;
                }

                // If a breach is registered, add it to the list
                if (!string.IsNullOrEmpty(breachSide))
                {
                    fundLayerBoundaryBreaches.Add(new()
                    {
                        TimeStamp = DateTime.Now,
                        FundId = fund.Id,
                        ItemId = fundLayerSettings.LayerIndex.ToString(),
                        FundName = fund.FundName,
                        ItemName = fundLayerSettings.Name,
                        SumRecord = layerDistribution.Value,
                        BoundarySide = breachSide,
                        BoundaryLevel = breachLevel,
                        CurrentLevel = currentPercentage
                    });
                }
            }
        }

        return fundLayerBoundaryBreaches;
    }


    /// <summary>
    /// Check all fund categories for boundary overrides. This method will 
    /// collect the current state of breaches and sends an alert when one or 
    /// more breaches have appeared.
    /// The state and alert state are saved in the AppSettings table.
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> FundCategoryBoundariesCheck(IServiceScope scope, CancellationToken cancellationToken)
    {
        var appConfigService = scope.ServiceProvider.GetRequiredService<IAppConfigService>();
        var notificationManager = scope.ServiceProvider.GetRequiredService<INotificationManager>();

        var currentAlertState = await appConfigService.GetAppConfigAsync(AppConfigs.FUND_CATEGORIES_BOUNDARIES_ALERT_STATE, string.Empty, cancellationToken);
        var storedBreaches = await appConfigService.GetAppConfigAsync(
                AppConfigs.FUND_CATEGORIES_BOUNDARIES_STATE,
                new List<FundBoundaryBreach>(),
                cancellationToken);
        var currentBreaches = await GenerateFundCategoryBreaches(scope, cancellationToken);

        // Check if the alert state is switched ON AND if there are no existing breach alerts
        if (!string.IsNullOrEmpty(currentAlertState) && currentBreaches.Count == 0)
        {
            // The alert state must be switched OFF
            _logger.LogInformation(LOG_INFO_FUND_CATEGORY_BOUNDARIES_ALERT_RESTORE, DateTime.Now);
            // " Delete" notification
            if (await notificationManager.DeleteNotification(new string[] { UserRoles.Admin }, Guid.Parse(currentAlertState), cancellationToken))
            {
                _ = await appConfigService.SetAppConfigAsync(AppConfigs.FUND_CATEGORIES_BOUNDARIES_STATE, null, string.Empty, cancellationToken);
                _ = await appConfigService.SetAppConfigAsync(AppConfigs.FUND_CATEGORIES_BOUNDARIES_ALERT_STATE, string.Empty, string.Empty, cancellationToken);
            }
        }
        // CHeck if there are more or less currently breached fund categories than there are stored in the db.
        else if (currentBreaches.Count > 0)
        {
            if (MergeAlertStates(storedBreaches, currentBreaches))
            {
                _ = await appConfigService.SetAppConfigAsync(AppConfigs.FUND_CATEGORIES_BOUNDARIES_STATE, currentBreaches, string.Empty, cancellationToken);

                if (string.IsNullOrEmpty(currentAlertState))
                {
                    _logger.LogWarning(LOG_INFO_FUND_CATEGORY_BOUNDARIES_ALERT, DateTime.Now);

                    // Notification sending
                    var notificationId = await notificationManager.SendNotification(
                        source: StackTraceHelper.CallingMethod(this),
                        type: NotificationType.Warning,
                        title: FUND_CATEGORIES_BOUNDARIES_CHECK_PROCESSNAME,
                        message: INFO_MESSAGE_FUND_CATEGORY_BOUNDARIES_ALERT,
                        info: JsonSerializer.Serialize(currentBreaches),
                        role: UserRoles.Admin,
                        cancellationToken: cancellationToken);

                    // Update alert state
                    _ = await appConfigService.SetAppConfigAsync(AppConfigs.FUND_CATEGORIES_BOUNDARIES_ALERT_STATE, notificationId.ToString(), string.Empty, cancellationToken);
                }
                else
                {
                    await notificationManager.UpdateNotification(
                        new string[] { UserRoles.Admin },
                        Guid.Parse(currentAlertState),
                        type: NotificationType.Warning,
                        title: FUND_CATEGORIES_BOUNDARIES_CHECK_PROCESSNAME,
                        message: INFO_MESSAGE_FUND_CATEGORY_BOUNDARIES_ALERT,
                        info: JsonSerializer.Serialize(currentBreaches),
                        role: UserRoles.Admin,
                        cancellationToken);
                }
            }
        }

        return true;
    }

    private static async Task<List<FundBoundaryBreach>> GenerateFundCategoryBreaches(IServiceScope scope, CancellationToken cancellationToken = default)
    {
        // Result list
        List<FundBoundaryBreach> fundCategoryBreachAlerts = new();

        var fundService = scope.ServiceProvider.GetRequiredService<IFundService>();

        foreach (Fund fund in await fundService.GetActiveFunds(cancellationToken))
        {
            var fundHoldings = await fundService.GetCurrentFundHoldings(fund.Id, true, true, cancellationToken);
            var fundCategories = await fundService.GetFundCategories(fund.Id, cancellationToken);
            var fundCategoryDistributions = await fundService.CalcCategoryDistribution(fundCategories, fundHoldings, cancellationToken);

            foreach (var fundCategoryDistribution in fundCategoryDistributions)
            {
                // Find the corresponding category
                var fundCategory = fundCategories.SingleOrDefault(fc => fc.CategoryId.Equals(fundCategoryDistribution.Key));

                // No category no breach, no limits set, also no breach
                if (fundCategory == null
                    || (fundCategory.MinPercentage == 0 && fundCategory.MaxPercentage == 0))
                    continue;

                string breachSide = string.Empty; // Empty means no breach
                int breachLevel = 0;
                int currentPercentage = (int)Math.Round(fundCategoryDistribution.Value.TotalSharePercentage);

                if (currentPercentage < fundCategory.MinPercentage)
                {
                    breachSide = "LOWER";
                    breachLevel = fundCategory.MinPercentage;
                }
                else if (currentPercentage > fundCategory.MaxPercentage)
                {
                    breachSide = "UPPER";
                    breachLevel = fundCategory.MaxPercentage;
                }

                if (!string.IsNullOrEmpty(breachSide))
                {
                    fundCategoryBreachAlerts.Add(new()
                    {
                        TimeStamp = DateTime.Now,
                        FundId = fund.Id,
                        ItemId = fundCategory.CategoryId.ToString(),
                        FundName = fund.FundName,
                        ItemName = $"{fundCategory.Category.Group}.{fundCategory.Category.Name}",
                        SumRecord = fundCategoryDistribution.Value,
                        BoundarySide = breachSide,
                        BoundaryLevel = breachLevel,
                        CurrentLevel = currentPercentage
                    });
                }
            }
        }

        return fundCategoryBreachAlerts;
    }

    #endregion FundBoundaryBreachesChecking



    /// <summary>
    /// Update the current breach list with the date from teh first breach. If 
    /// anything changed, the return value will be true. When nothing changed, 
    /// false is returned.
    /// </summary>
    /// <param name="savedBreaches"></param>
    /// <param name="currentBreaches"></param>
    /// <returns>True when state is changed, false state stays the same.</returns>
    private static bool MergeAlertStates(IList<FundBoundaryBreach> savedBreaches, IList<FundBoundaryBreach> currentBreaches)
    {
        // First check if any breach from the saved state is removed, that si a state change.
        bool stateChanged = savedBreaches
            .Any(b_saved => !currentBreaches
            .Any(b_current => b_current.FundId.Equals(b_saved.FundId) && b_current.ItemId.Equals(b_saved.ItemId)));

        // Update the datetime stamps for the items in both lists
        foreach (var breach in currentBreaches)
        {
            var stored = savedBreaches.SingleOrDefault(b => b.FundId.Equals(breach.FundId) && b.ItemId.Equals(breach.ItemId));

            if (stored != null)
            {
                breach.TimeStamp = stored.TimeStamp;
                stateChanged |= breach.CurrentLevel != stored.CurrentLevel;
            }
            else
            {
                stateChanged = true;
            }
        }

        return stateChanged;
    }
}
