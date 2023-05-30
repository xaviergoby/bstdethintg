namespace Hodl.ExchangeAPI.Models;

public class ExchangeBalance
{
    /// <summary>
    /// The asset this balance is for
    /// </summary>
    public string Asset { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// The quantity that isn't locked in a trade
    /// </summary>
    public decimal Available { get; set; }
    /// <summary>
    /// The quantity that is currently locked in a trade
    /// </summary>
    public decimal Locked { get; set; }
    /// <summary>
    /// The total balance of this asset (normally Free + Locked)
    /// </summary>
    public decimal Total { get; set; }

    public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;
}
