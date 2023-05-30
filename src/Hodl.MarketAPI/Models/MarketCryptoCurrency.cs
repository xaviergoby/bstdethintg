namespace Hodl.MarketAPI.Models;

public class MarketCryptoCurrency
{
    private string _symbol = string.Empty;

    public byte Decimals { get; set; } = 9;

    public string Symbol
    {
        get { return _symbol; }
        set { _symbol = value.ToUpperInvariant(); }
    }

    public string Name { get; set; } = string.Empty;

    public bool Active { get; set; }

    public bool IsStableCoin { get; set; }

    public bool IsLocked { get; set; }

    public byte[] Icon { get; set; }

    public override string ToString() => Name;
}
