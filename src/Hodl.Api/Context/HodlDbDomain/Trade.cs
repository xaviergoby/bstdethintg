using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(OrderId), nameof(DateTime))]
[Index(nameof(OrderId), nameof(BookingPeriod))]
[Index(nameof(OrderId), nameof(ExchangeTradeId))]
public class Trade
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }

    [ForeignKey("OrderId")]
    public Order Order { get; set; }

    [MaxLength(64), AllowNull]
    public string ExchangeTradeId { get; set; }

    [MaxLength(128)]
    public string TransactionId { get; set; }

    public DateTime DateTime { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Executed { get; set; }

    public decimal Total { get; set; }

    public decimal Fee { get; set; }

    [Required]
    public Guid FeeCurrencyId { get; set; }

    [ForeignKey("FeeCurrencyId")]
    public CryptoCurrency FeeCurrency { get; set; }

    [MinLength(6), MaxLength(6), AllowNull]
    [Column(TypeName = "nchar(6)")]
    public string BookingPeriod { get; set; }
}
