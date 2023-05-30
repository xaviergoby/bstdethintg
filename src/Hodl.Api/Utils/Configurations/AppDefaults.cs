namespace Hodl.Api.Utils.Configurations;

public class AppDefaults
{
    private static readonly string[] _testEnvironments = new string[] { "TEST", "DEV", "LOCALDEV" };

    public string RunningEnvironment { get; set; } = "TEST";

    public bool IsTestEnvironment() => _testEnvironments.Contains(RunningEnvironment);

    public int CryptoListingUpdateInSeconds { get; set; } = 300;

    public int CryptoListingCleanupInDays { get; set; } = 7;

    public int CurrencyRatesUpdateInSeconds { get; set; } = 60; // 900

    public int CryptoListingHistoryIntervalInSeconds { get; set; } = 3600;

    public int CryptoListingHistoryNumberOfDays { get; set; } = 365;

    public int FundLayersCheckIntervalInSeconds { get; set; } = 300;

    public int FundCategoriesCheckIntervalInSeconds { get; set; } = 300;

    public int ExchangeTradeEventsUpdateInSeconds { get; set; } = 300;

    public int DailyNavHour { get; set; } = 16;

    public string BookingPeriod { get; set; } = "Monthly";

    public int CloseBookingPeriodHour { get; set; } = 16;

    public string ReportingTimeZone { get; set; } = "W. Europe Standard Time";

    public string ReportingLocalization { get; set; } = "nl-NL";
}
