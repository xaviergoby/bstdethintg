namespace Hodl.Api.Interfaces;

public interface INotificationHandler
{
    Task SendNotification(NotificationMessage notification, CancellationToken cancellationToken = default);
}

