namespace Hodl.Api.Utils.Notifications.Handlers;

public class EmailNotificationHandler : INotificationHandler
{
    private const string EMAIL_BASE_SUBJECT = "Notification Service: {0}";
    private const string EMAIL_BASE_MESSAGE_CONTENT = "<br>{0}</br>{1}";

    // The dependent interface 
    private readonly IEmailUserGroupService _emailUserGroupService;

    public EmailNotificationHandler(IEmailUserGroupService emailUserGroupService)
    {
        _emailUserGroupService = emailUserGroupService;
    }

    public async Task SendNotification(NotificationMessage notification, CancellationToken cancellationToken)
    {
        await _emailUserGroupService.SendAlert(
            userRole: notification.NormalizedRoleName,
            subject: string.Format(EMAIL_BASE_SUBJECT, notification.Title),
            message: string.Format(EMAIL_BASE_MESSAGE_CONTENT, notification.Message, notification.Url),
            cancellationToken);
    }
}

