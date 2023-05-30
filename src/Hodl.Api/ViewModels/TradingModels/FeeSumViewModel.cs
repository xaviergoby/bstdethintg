using Hodl.Api.ViewModels.CurrencyModels;

namespace Hodl.Api.ViewModels.TradingModels;

public class FeeSumViewModel
{
    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal FeeSum { get; set; }

    public CryptoCurrencyListViewModel FeeCurrency { get; set; }
}
