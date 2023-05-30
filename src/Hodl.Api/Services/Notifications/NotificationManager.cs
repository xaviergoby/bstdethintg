using System.Web;

namespace Hodl.Api.Services.Notifications;

/// <summary>
/// The NotificationManager cls encapsulates TradingDesk business domain logic
/// As such, it is the layer in between the "Presentation layer", i.e. API (Controller) Endpoints,
/// & the "Data (access) Layer" , i.e. EF Core
/// </summary>
public class NotificationManager : INotificationManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAppConfigService _appConfigService;
    private readonly HodlDbContext _dbContext;
    private readonly IMapper _mapper;


    private readonly List<INotificationHandler> notificationHandlers = new();

    public NotificationManager(IServiceProvider serviceProvider,
        IAppConfigService appConfigService,
        HodlDbContext dbContext,
        IMapper mapper
        )
    {
        _serviceProvider = serviceProvider;
        _appConfigService = appConfigService;
        _dbContext = dbContext;
        _mapper = mapper;

        foreach (var handler in _serviceProvider.GetServices<INotificationHandler>())
        {
            RegisterNotificationHandler(handler);
        }
    }

    private void RegisterNotificationHandler(INotificationHandler notificationHandler)
    {
        notificationHandlers.Add(notificationHandler);
    }

#pragma warning disable IDE0051
    private void UnregisterNotificationHandler(INotificationHandler notificationHandler)
    {
        notificationHandlers.Remove(notificationHandler);
    }
#pragma warning restore IDE0051

    private async Task<NotificationMessage> CreateNotification(
        string source,
        NotificationType type,
        string title,
        string message,
        string info,
        string role,
        CancellationToken cancellationToken = default)
    {
        // Generate notification instance for the fund layer breach
        var notification = new Notification()
        {
            Source = source,
            Timestamp = DateTime.UtcNow,
            Type = type,
            Title = title,
            Message = message,
            Info = info,
            NormalizedRoleName = role,
        };


        await _dbContext.Notifications.AddAsync(notification, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<NotificationMessage>(notification);
    }

    private async Task<string> GetNotificationUrl(Guid id, CancellationToken cancellationToken = default)
    {
        var notificationUrlBasePath = await _appConfigService.GetAppConfigAsync(AppConfigs.NOTIFICATION_URL_BASE_PATH, string.Empty, cancellationToken);

        return HttpUtility.UrlEncode(notificationUrlBasePath.Replace("{notification_id}", id.ToString()));
    }

    /// <summary>
    /// This method of this service is meant to be called, e.g. by 
    /// FundLayerCheck.cs, when a notification needs to be dealt with by the 
    /// various handlers subscribed to this NotificationService publisher.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Guid> SendNotification(
        string source,
        NotificationType type,
        string title,
        string message,
        string info,
        string role,
        CancellationToken cancellationToken = default)
    {
        var notification = await CreateNotification(source, type, title, message, info, role, cancellationToken);
        notification.Url = await GetNotificationUrl(notification.Id, cancellationToken);

        foreach (var handler in notificationHandlers)
        {
            await handler.SendNotification(notification, cancellationToken);
        }

        return notification.Id;
    }

    /// <summary>
    /// </summary>
    /// <param name="notificationId"></param>
    /// <param name="type"></param>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="info"></param>
    /// <param name="role"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<NotificationMessage> UpdateNotification(
        IEnumerable<string> roles,
        Guid notificationId,
        NotificationType type,
        string title,
        string message,
        string info,
        string role,
        CancellationToken cancellationToken = default)
    {
        var notification = await GetNotificationById(roles, notificationId, cancellationToken) ??
            throw new NotFoundException($"Notification not found: {notificationId}.");

        notification.Type = type;
        notification.Title = title;
        notification.Message = message;
        notification.Info = info;
        notification.NormalizedRoleName = role;

        _ = await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<NotificationMessage>(notification);
    }

    /// <summary>
    /// </summary>
    /// <param name="notificationId"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<bool> DeleteNotification(
        IEnumerable<string> roles,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = await GetNotificationById(roles, notificationId, cancellationToken) ??
            throw new NotFoundException($"Notification not found: {notificationId}.");

        notification.IsDeleted ??= DateTime.UtcNow; // Only set when empty

        _ = await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// This is a "helper" method for querying the db for all notifications
    /// The NotificationController cls depends on this method
    /// </summary>
    /// <param name="notificationId"></param>
    /// <returns></returns>
    public async Task<PagingModel<NotificationMessage>> GetAllNotitifications(
        IEnumerable<string> roles,
        int page,
        int? itemsPerPage,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Notifications
            .AsNoTracking()
            .Where(n => roles.Contains(n.NormalizedRoleName))
            .OrderByDescending(n => n.Timestamp)
            .Select(n => _mapper.Map<NotificationMessage>(n));

        return await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, cancellationToken);
    }

    /// <summary>
    /// This is a "helper" method querying the db for a notification
    /// The NotificationController cls depends on this method
    /// </summary>
    /// <param name="notificationId"></param>
    /// <returns></returns>
    public async Task<NotificationMessage> GetNotificationById(
        IEnumerable<string> roles,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.Id == notificationId && roles.Contains(n.NormalizedRoleName))
            .Select(n => _mapper.Map<NotificationMessage>(n))
            .FirstOrDefaultAsync(cancellationToken) ??
            throw new NotFoundException($"Notification not found: {notificationId}.");
    }
}
