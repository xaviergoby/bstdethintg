namespace Hodl.MarketAPI.Configurations;

public class CoinGeckoOptions
{
    public string ApiKey { get; set; }

    public int MinuteLimit { get; set; } = 50; // Default free

    public int MonthlyLimit { get; set; }
}
