using Hodl.Api.ViewModels.ExternalAccountModels;
using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.FundModels;

public class FundDetailViewModel
{
    public FundOwnerListViewModel FundOwner { get; set; }

    [Required(ErrorMessage = "Fund name is required")]
    public string FundName { get; set; }

    public string Description { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime? DateEnd { get; set; }

    public int MaxVolume { get; set; }

    public string LayerStrategy { get; set; }

    public string ReportingCurrencyCode { get; set; }

    public Guid PrimaryCryptoCurrencyId { get; set; }

    public int AdministrationFee { get; set; }

    public int PerformanceFee { get; set; }

    public int AdministrationFeeFrequency { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public decimal TotalValue { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader,HeadSales")]
    public int TotalShares { get; set; }

    public decimal ShareValueHWM { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public bool CloseHistoryButton { get; set; }

    public string CurrentBookingPeriod { get; set; }

    public virtual PeriodNavViewModel Nav { get; set; }

    public virtual ICollection<DailyNavViewModel> DailyNavs { get; set; }

    public virtual ICollection<FundLayerViewModel> Layers { get; set; }

    public virtual ICollection<FundCategoryViewModel> FundCategories { get; set; }

    [VisibleForRoles(Roles = "Admin,LeadTrader")]
    public virtual ICollection<BankAccountListViewModel> BankAccounts { get; set; }
}
