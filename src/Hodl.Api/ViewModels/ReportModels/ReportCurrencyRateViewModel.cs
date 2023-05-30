namespace Hodl.Api.ViewModels.ReportModels;

public class ReportCurrencyRateViewModel
{
    public long Id { get; set; }

    public string CurrencyISOCode { get; set; }

    public decimal USDRate { get; set; }

    public DateTime TimeStamp { get; set; }

    public string Source { get; set; }
}
