using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.HodlDbDomain;

public class Currency
{
    private string _isoCode;

    [Key, MaxLength(3)]
    public string ISOCode
    {
        get { return _isoCode; }
        set { _isoCode = value.ToUpperInvariant(); }
    }

    public byte Decimals { get; set; }

    [MaxLength(5)]
    public string Symbol { get; set; }

    [MaxLength(40)]
    public string Name { get; set; }

    [MaxLength(120)]
    public string Location { get; set; }

    public bool Default { get; set; }

    public bool Active { get; set; }

    public virtual ICollection<CurrencyRate> CurrencyRates { get; set; }

    public virtual ICollection<Holding> Holdings { get; set; }

    public override string ToString() => Name;
}
