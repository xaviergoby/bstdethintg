using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.HodlDbDomain;

public class FundOwner
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(60)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [MaxLength(60)]
    public string Department { get; set; } = string.Empty;

    [MaxLength(60)]
    public string Country { get; set; }

    public virtual ICollection<Fund> Funds { get; set; }

    public override string ToString() => Name;
}
