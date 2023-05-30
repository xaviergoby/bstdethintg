using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.ExternalAccountModels;

public class WalletEditViewModel
{
    [Key, MaxLength(128)]
    public string Address { get; set; }

    public Guid? ExchangeAccountId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string Description { get; set; }
}
