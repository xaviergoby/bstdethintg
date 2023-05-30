namespace Hodl.ExplorerAPI.Models;

public class TransactionModel
{
    public string TransactionHash { get; set; }

    public string ExplorerUrl { get; set; }

    public DateTimeOffset TimeStamp { get; set; }

    public string BlockHash { get; set; }

    public string BlockNumber { get; set; }

    public string From { get; set; }

    public string Gas { get; set; }

    public string GasPrice { get; set; }

    public string Hash { get; set; }

    public string To { get; set; }

    public string Value { get; set; }

}
