namespace Hodl.MarketAPI.Configurations;

public class CoinmarketCapOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public int DailyCredits { get; set; } = 333;

    public int MonthlyCredits { get; set; } = 10000;
}
