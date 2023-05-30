using Hodl.Api.Controllers.Currencies;
using Hodl.Api.ViewModels.NotificationModels;

namespace Hodl.Api.Controllers.Notification;

[ApiController]
[Route("notifications")]
public class NotificationsController : BaseController
{
    private readonly INotificationManager _notificationManager;
    private readonly IUserResolver _userResolver;

    public NotificationsController(
        INotificationManager notificationManager,
        IUserResolver userResolver,
        IMapper mapper,
        ILogger<CryptoCurrencyController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)

    {
        _notificationManager = notificationManager;
        _userResolver = userResolver;
    }

    /// <summary>
    /// Send a test message to all message handlers.
    /// </summary>
    /// <param name="notificationId"></param>
    /// <param name="notificationMessage"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("/send")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendNotification(string notificationMessage)
    {
        var msgId = await _notificationManager.SendNotification(
            source: StackTraceHelper.CallingMethod(this),
            type: NotificationType.Warning,
            title: "Test message",
            message: notificationMessage,
            info: notificationMessage,
            role: UserRoles.Admin);

        _ = await _notificationManager.DeleteNotification(new string[] { UserRoles.Admin }, msgId);

        return Ok();
    }

    /// <summary>
    /// </summary>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("notifications")]
    public async Task<ActionResult<PagingViewModel<NotificationListViewModel>>> GetAllNotitifications(int page, int? itemsPerPage, CancellationToken ct)
    {
        var user = await _userResolver.GetUser() ??
            throw new RestException(HttpStatusCode.Unauthorized, "User not logged in");

        var pageResult = await _notificationManager.GetAllNotitifications(user.Roles, page, itemsPerPage, ct);

        return Ok(_mapper.Map<PagingViewModel<NotificationListViewModel>>(pageResult));
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="notificationId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{notificationId}")]
    public async Task<ActionResult<NotificationDetailViewModel>> GetNotificationById(Guid notificationId, CancellationToken ct)
    {
        var user = await _userResolver.GetUser() ??
            throw new RestException(HttpStatusCode.Unauthorized, "User not logged in");

        var notification = await _notificationManager.GetNotificationById(user.Roles, notificationId, ct);

        return Ok(_mapper.Map<NotificationDetailViewModel>(notification));
    }


    /// <summary>
    /// </summary>
    /// <param name="notificationId"></param>
    /// <param name="notification"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("{notificationId}")]
    public async Task<IActionResult> EditNotification(Guid notificationId, [FromBody] NotificationEditViewModel notification)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data.");

        var user = await _userResolver.GetUser() ??
            throw new RestException(HttpStatusCode.Unauthorized, "User not logged in");

        var storedNotification = await _notificationManager.GetNotificationById(user.Roles, notificationId);

        if (storedNotification == null)
            return NotFound();

        var updatedNotification = _mapper.Map(notification, storedNotification);

        await _notificationManager.UpdateNotification(user.Roles,
            notificationId,
            updatedNotification.Type,
            updatedNotification.Title,
            updatedNotification.Message,
            updatedNotification.Info,
            updatedNotification.NormalizedRoleName);

        return Ok(updatedNotification);

    }


    [HttpDelete]
    [Route("{notificationId}")]
    public async Task<IActionResult> DeleteNotification(Guid notificationId)
    {
        var user = await _userResolver.GetUser() ??
            throw new RestException(HttpStatusCode.Unauthorized, "User not logged in");

        await _notificationManager.DeleteNotification(user.Roles, notificationId);

        return Ok();
    }
}