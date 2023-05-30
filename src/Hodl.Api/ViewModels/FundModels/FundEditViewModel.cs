using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.FundModels;

public class FundEditViewModel
{
    public Guid? FundOwnerId { get; set; }

    [Required(ErrorMessage = "Fund name is required")]
    public string FundName { get; set; }

    public string Description { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime? DateEnd { get; set; }

    public int MaxVolume { get; set; }

    public string LayerStrategy { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public string ReportingCurrencyCode { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public Guid PrimaryCryptoCurrencyId { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public int AdministrationFee { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public int AdministrationFeeFrequency { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public int PerformanceFee { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public decimal TotalValue { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public int TotalShares { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal ShareValueHWM { get; set; }
}
