using System.ComponentModel.DataAnnotations.Schema;

namespace Hodl.Api.HodlDbDomain;

[Index(nameof(OrderId), nameof(FundId), IsUnique = true)]
public class OrderFunding
{
    public Guid OrderId { get; set; }

    [ForeignKey("OrderId")]
    public Order Order { get; set; }

    public Guid FundId { get; set; }

    [ForeignKey("FundId")]
    public Fund Fund { get; set; }

    public decimal OrderPercentage { get; set; }

    public decimal OrderAmount { get; set; }

    public decimal OrderTotal { get; set; }
}
