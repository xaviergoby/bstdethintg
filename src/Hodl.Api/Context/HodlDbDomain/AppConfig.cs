using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Table("AppConfigs")]
public class AppConfig
{
    [Key, MaxLength(64)]
    public string Name { get; set; }

    public string Value { get; set; }

    public DateTime DateTime { get; set; }

    [MaxLength(256)]
    public string NormalizedRoleName { get; set; }
}
