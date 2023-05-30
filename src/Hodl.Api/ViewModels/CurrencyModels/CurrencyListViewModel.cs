using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.CurrencyModels;

public class CurrencyListViewModel
{
    [Key, MaxLength(3)]
    public string ISOCode { get; set; }

    public byte Decimals { get; set; }

    [MaxLength(5)]
    public string Symbol { get; set; }

    [MaxLength(40)]
    public string Name { get; set; }

    [MaxLength(120)]
    public string Location { get; set; }

    public bool Default { get; set; } = false;

    public bool Active { get; set; } = true;

    public virtual CurrencyRateViewModel CurrencyRate { get; set; }
}
