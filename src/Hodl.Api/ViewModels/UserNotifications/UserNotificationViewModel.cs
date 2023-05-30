namespace Hodl.Api.ViewModels.UserNotifications;

public class UserNotificationViewModel
{
    public Guid Id { get; set; }

    public bool IsRead { get; set; }

    public long Timestamp { get; set; }

    public string Date { get; set; }

    public string Time { get; set; }

    public Guid EntityId { get; set; }

    public string EntityTitle { get; set; }

    public string NotificationType { get; set; }

    public string Description { get; set; }

    public string Message { get; set; }

    public string OptionalData { get; set; }
}
