using Hodl.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Models;

public class NotificationMessage
{
    public Guid Id { get; set; } = new();

    public DateTime Timestamp { get; set; }

    public NotificationType Type { get; set; }

    [MaxLength(128)]
    public string Source { get; set; }

    [MaxLength(128)]
    public string Title { get; set; }

    [MaxLength(256)]
    public string Message { get; set; }

    public string Info { get; set; }

    [MaxLength(64)]
    public string NormalizedRoleName { get; set; }

    public DateTime? IsDeleted { get; set; } = null;

    [NotMapped]
    public string Url { get; set; }
}