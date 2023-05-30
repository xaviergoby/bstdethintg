using Hodl.Api.ViewModels.CurrencyModels;

namespace Hodl.Api.ViewModels.FundModels;

public class DailyNavViewModel
{
    public Guid Id { get; set; }

    public DateTime DateTime { get; set; }

    public DateTime Date { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal TotalValue { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public int TotalShares { get; set; }

    public decimal ShareHWM { get; set; }

    public decimal ShareGross { get; set; }

    public decimal ShareNAV { get; set; }

    public CurrencyRateViewModel CurrencyRate { get; set; }
}
