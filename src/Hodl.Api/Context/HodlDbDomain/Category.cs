using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(Group), nameof(Name))]
public class Category
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(40)]
    public string Name { get; set; }

    public string Description { get; set; }

    [MaxLength(128)]
    public string Group { get; set; }

    public bool IncludeFiat { get; set; } = false;

    public virtual ICollection<CryptoCurrency> CryptoCurrencies { get; set; }
    public virtual ICollection<CryptoCategory> CryptoCategories { get; set; }
    public virtual ICollection<Fund> Funds { get; set; }
    public virtual ICollection<FundCategory> FundCategories { get; set; }

    public override string ToString() => Name;

}
