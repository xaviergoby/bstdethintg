namespace Hodl.ExplorerAPI.Models;

public class ExplorerBalance
{
    public string WalletAddress { get; set; }

    public string ExplorerUrl { get; set; }

    public string CurrencySymbol { get; set; }

    public string CurrencyName { get; set; }

    public decimal Balance { get; set; }

    public DateTimeOffset TimeStamp { get; set; }
}
