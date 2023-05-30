namespace Hodl.Api.ViewModels.TradingModels;

public class TradeAddViewModel
{
    public string TransactionId { get; set; }

    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    public decimal UnitPrice { get; set; }

    public decimal Executed { get; set; }

    public decimal Fee { get; set; }

    public Guid FeeCurrencyId { get; set; }
}
