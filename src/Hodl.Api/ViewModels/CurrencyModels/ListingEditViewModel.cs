using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.CurrencyModels;

public class ListingEditViewModel
{
    [MaxLength(128)]
    public string Source { get; set; }

    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    public int CmcRank { get; set; }

    public decimal CirculatingSupply { get; set; }

    public decimal TotalSupply { get; set; }

    public decimal MaxSupply { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal USDPrice { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal BTCPrice { get; set; }

    public decimal Volume_24h { get; set; }

    public decimal PercentChange_1h { get; set; }

    public decimal PercentChange_24h { get; set; }

    public decimal PercentChange_7d { get; set; }

    public decimal MarketCap { get; set; }
}
