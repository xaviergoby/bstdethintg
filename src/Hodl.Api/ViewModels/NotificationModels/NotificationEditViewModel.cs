using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.NotificationModels;

public class NotificationEditViewModel
{
    public NotificationType Type { get; set; }

    [MaxLength(128)]
    public string Title { get; set; }

    [MaxLength(256)]
    public string Message { get; set; }

    public string Info { get; set; }

    [MaxLength(64)]
    public string NormalizedRoleName { get; set; }

    public DateTime? IsDeleted { get; set; }
}
