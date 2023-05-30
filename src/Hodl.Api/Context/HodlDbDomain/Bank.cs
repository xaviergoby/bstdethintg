using System.ComponentModel.DataAnnotations;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(BIC), IsUnique = true)]
public class Bank
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(60)]
    public string Name { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(11)]
    public string BIC { get; set; } = string.Empty;

    [MaxLength(60)]
    public string City { get; set; }

    [MaxLength(60)]
    public string Branch { get; set; }

    [MaxLength(120)]
    public string Address { get; set; }

    [MaxLength(10)]
    public string Zipcode { get; set; }

    [MaxLength(60)]
    public string Country { get; set; }

    [MaxLength(2)]
    public string CountryCode { get; set; }

    [MaxLength(256)]
    public string Url { get; set; }

    public virtual ICollection<BankAccount> BankAccounts { get; set; }

    public override string ToString() => $"{Name} ({BIC})";
}
