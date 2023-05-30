using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.HodlDbDomain;

public class UserNotification
{
    [Key]
    public Guid Id { get; set; }

    public string OwnerId { get; set; }

    public bool IsRead { get; set; }

    public long Timestamp { get; set; }

    public Guid EntityId { get; set; }

    public string EntityTitle { get; set; }

    public NotificationType NotificationType { get; set; }

    [MaxLength(20)]
    public string DescriptionCode { get; set; }

    public string DescriptionMessage { get; set; }

    public string OptionalData { get; set; }
}
