using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Table("ChangeLog")]
[Index(nameof(TableName), nameof(DateTime))]
[Index(nameof(NormalizedRoleName), nameof(TableName), nameof(DateTime))]
public class AppChangeLog
{
    [Key]
    public long Id { get; set; }

    [Required]
    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    public Guid? UserId { get; set; }

    [ForeignKey("UserId")]
    public AppUser User { get; set; }

    [MaxLength(256)]
    public string NormalizedUserName { get; set; }

    [MaxLength(256)]
    public string NormalizedRoleName { get; set; }

    [MaxLength(256)]
    public string TableName { get; set; }

    public string OldRecord { get; set; }

    public string NewRecord { get; set; }
}
