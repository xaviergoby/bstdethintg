using CoinGeckoAPI.Entities.Response.ExchangeRates;

namespace CoinGeckoAPI.Interfaces;

public interface IExchangeRatesClient
{
    /// <summary>
    /// Get BTC-to-Currency exchange rates
    /// </summary>
    /// <returns></returns>
    Task<ExchangeRates> GetExchangeRates();
}