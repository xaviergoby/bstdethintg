using Hodl.Api.ViewModels.TradingModels;
using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.ExternalAccountModels;

public class WalletDetailViewModel
{
    [Key]
    public string Address { get; set; }

    public ExchangeAccountListViewModel ExchangeAccount { get; set; }

    public DateTime Timestamp { get; set; }

    public string Description { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public virtual ICollection<WalletBalanceListViewModel> WalletBalances { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public virtual ICollection<OrderListViewModel> Orders { get; set; }
}
