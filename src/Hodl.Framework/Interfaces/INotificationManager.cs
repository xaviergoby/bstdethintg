using Hodl.Constants;
using Hodl.Models;

namespace Hodl.Interfaces;

public interface INotificationManager
{
    Task<Guid> SendNotification(string source, NotificationType type, string title, string message, string info, string role, CancellationToken cancellationToken = default);

    Task<NotificationMessage> UpdateNotification(IEnumerable<string> roles, Guid id, NotificationType type, string title, string message, string info, string role, CancellationToken cancellationToken = default);

    Task<bool> DeleteNotification(IEnumerable<string> roles, Guid id, CancellationToken cancellationToken = default);

    Task<PagingModel<NotificationMessage>> GetAllNotitifications(IEnumerable<string> roles, int page, int? itemsPerPage, CancellationToken cancellationToken = default);

    Task<NotificationMessage> GetNotificationById(IEnumerable<string> roles, Guid notificationId, CancellationToken cancellationToken = default);
}