namespace Hodl.MarketAPI;

public interface IFiatMarketAPI
{
    string Source { get; }

    DateTime DateTime { get; }

    Task<decimal> ExchangeRate(string baseCurrency, string currency);
}
