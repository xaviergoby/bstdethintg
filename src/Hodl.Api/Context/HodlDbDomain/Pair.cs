using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(QuoteAssetId), nameof(BaseAssetId))]
public class Pair
{
    [Key, MaxLength(12)]
    public string PairString { get; set; }

    [Required]
    public Guid BaseAssetId { get; set; }

    /// <summary>
    /// The Base Asset is the first currency in the pair
    /// </summary>
    [ForeignKey("BaseAssetId")]
    public CryptoCurrency BaseAsset { get; set; }

    [Required]
    public Guid QuoteAssetId { get; set; }

    /// <summary>
    /// The Quote Asset is the last currency in the pair
    /// </summary>
    [ForeignKey("QuoteAssetId")]
    public CryptoCurrency QuoteAsset { get; set; }

    public override string ToString() => PairString;
}
