namespace Hodl.ExchangeAPI.Models;

public class ExchangeTrade
{
    public string Id { get; set; }

    public string OrderId { get; set; }

    public string TransactionId { get; set; }

    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    public decimal UnitPrice { get; set; }

    public decimal Executed { get; set; }

    public decimal Total { get; set; }

    public decimal Fee { get; set; }

    public ExchangeCryptoCurrency FeeAsset { get; set; }
}
