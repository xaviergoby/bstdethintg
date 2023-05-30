namespace Hodl.Api.ViewModels.FundModels;

public class FundListViewModel
{
    public Guid Id { get; private set; }

    public FundOwnerListViewModel FundOwner { get; set; }

    public string FundName { get; set; }

    public string NormalizedFundName { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime? DateEnd { get; set; }

    public int MaxVolume { get; set; }

    public int AdministrationFee { get; set; }

    public int AdministrationFeeFrequency { get; set; }

    public int PerformanceFee { get; set; }

    public decimal TotalValue { get; set; }

    public int TotalShares { get; set; }

    public decimal ShareValueHWM { get; set; }

    public string CurrentBookingPeriod { get; set; }

    public PeriodNavViewModel Nav { get; set; }
}
