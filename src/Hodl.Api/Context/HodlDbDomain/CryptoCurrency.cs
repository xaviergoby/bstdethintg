using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(Symbol), nameof(Name), IsUnique = true)]
public class CryptoCurrency
{
    private string _symbol = string.Empty;

    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public byte Decimals { get; set; } = 9;

    [Required, MaxLength(8)]
    public string Symbol
    {
        get { return _symbol; }
        set { _symbol = value.ToUpperInvariant(); }
    }

    [Required, MaxLength(40)]
    public string Name { get; set; } = string.Empty;

    public bool Active { get; set; }

    public bool IsStableCoin { get; set; }

    public bool IsLocked { get; set; }

    public byte[] Icon { get; set; }

    public Guid? ListingCryptoId { get; set; }

    [ForeignKey("ListingCryptoId")]
    public CryptoCurrency ListingReference { get; set; }

    public virtual ICollection<Listing> Listings { get; set; }

    public virtual ICollection<Category> Categories { get; set; }

    public virtual ICollection<Holding> Holdings { get; set; }

    public override string ToString() => Name;
}
