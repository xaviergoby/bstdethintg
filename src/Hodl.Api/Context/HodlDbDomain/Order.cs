using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(BaseAssetId), nameof(Type), nameof(DateTime))]
[Index(nameof(BaseAssetId), nameof(State))]
[Index(nameof(QuoteAssetId), nameof(Type), nameof(DateTime))]
[Index(nameof(QuoteAssetId), nameof(State))]
public class Order
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(64)]
    public string ExchangeOrderId { get; set; }

    [MaxLength(64)]
    public string ExchangeClientOrderId { get; set; }

    public Guid? ExchangeAccountId { get; set; }

    [ForeignKey("ExchangeAccountId")]
    public ExchangeAccount ExchangeAccount { get; set; }

    [MaxLength(128)]
    public string WalletAddress { get; set; }

    [ForeignKey("WalletAddress")]
    public Wallet Wallet { get; set; }

    [MaxLength(64)]
    public string OrderNumber { get; set; }

    [Required]
    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid BaseAssetId { get; set; }

    [ForeignKey("BaseAssetId")]
    public CryptoCurrency BaseAsset { get; set; }

    [Required]
    public Guid QuoteAssetId { get; set; }

    [ForeignKey("QuoteAssetId")]
    public CryptoCurrency QuoteAsset { get; set; }

    [MaxLength(20)]
    public string Type { get; set; }

    public OrderDirection Direction { get; set; }

    public OrderState State { get; set; } = OrderState.New;

    public decimal UnitPrice { get; set; }

    public decimal Amount { get; set; }

    public decimal Total { get; set; }

    public bool IsTaker { get; set; } = true;

    public bool IsMaker { get; set; } = false;

    public Guid? AvgPriceCryptoId { get; set; }

    [ForeignKey("AvgPriceCryptoId")]
    public CryptoCurrency AvgPriceQuoteAsset { get; set; }

    public decimal AvgPriceRate { get; set; }

    public string AvgPriceSource { get; set; }

    public virtual ICollection<OrderFunding> OrderFundings { get; set; }
    public virtual ICollection<Fund> Funds { get; set; }
    public virtual ICollection<Trade> Trades { get; set; }
}
