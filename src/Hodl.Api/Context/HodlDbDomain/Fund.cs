using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(NormalizedFundName), IsUnique = true)]
public class Fund
{
    private string _fundName;

    [Key]
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid? FundOwnerId { get; set; }

    [ForeignKey("FundOwnerId")]
    public FundOwner FundOwner { get; set; }

    [Required, StringLength(40)]
    public string FundName
    {
        get => _fundName;
        set
        {
            _fundName = value;
            NormalizedFundName = value.ToUpperInvariant();
        }
    }

    [Required, StringLength(40)]
    public string NormalizedFundName { get; set; }

    public string Description { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime? DateEnd { get; set; }

    public int MaxVolume { get; set; }

    public string LayerStrategy { get; set; }

    [MaxLength(3)]
    public string ReportingCurrencyCode { get; set; }

    [ForeignKey("ReportingCurrencyCode")]
    public Currency ReportingCurrency { get; set; }

    public Guid PrimaryCryptoCurrencyId { get; set; }

    [ForeignKey("PrimaryCryptoCurrencyId")]
    public CryptoCurrency PrimaryCryptoCurrency { get; set; }

    [Required]
    public int AdministrationFee { get; set; } = 2;

    [Required]
    public int AdministrationFeeFrequency { get; set; } = 4;

    [Required]
    public int PerformanceFee { get; set; } = 20;

    public decimal TotalValue { get; set; }

    public int TotalShares { get; set; }

    public decimal ShareValueHWM { get; set; }

    public virtual ICollection<FundLayer> Layers { get; set; }
    public virtual ICollection<Category> Categories { get; set; }
    public virtual ICollection<FundCategory> FundCategories { get; set; }
    public virtual ICollection<Holding> Holdings { get; set; }
    public virtual ICollection<Nav> DailyNavs { get; set; }
    public virtual ICollection<Order> Orders { get; set; }
    public virtual ICollection<OrderFunding> OrderFundings { get; set; }
    public virtual ICollection<BankAccount> BankAccounts { get; set; }

    public override string ToString() => FundName;
}
