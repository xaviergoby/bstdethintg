using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.ExternalAccountModels;

public class WalletListViewModel
{
    [Key]
    public string Address { get; set; }

    public ExchangeAccountListViewModel ExchangeAccount { get; set; }

    public DateTime Timestamp { get; set; }

    public string Description { get; set; }
}
