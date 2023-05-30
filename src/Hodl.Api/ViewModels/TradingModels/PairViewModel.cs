using Hodl.Api.ViewModels.CurrencyModels;
using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.ViewModels.TradingModels;

public class PairViewModel
{
    [Key, MaxLength(12)]
    public string PairString { get; set; }

    /// <summary>
    /// The Base Asset is the first currency in the pair
    /// </summary>
    public CryptoCurrencyListViewModel BaseAsset { get; set; }

    /// <summary>
    /// The Quote Asset is the last currency in the pair
    /// </summary>
    public CryptoCurrencyListViewModel QuoteAsset { get; set; }
}
