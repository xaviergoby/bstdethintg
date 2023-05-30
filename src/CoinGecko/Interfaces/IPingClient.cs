using CoinGeckoAPI.Entities.Response;

namespace CoinGeckoAPI.Interfaces;

public interface IPingClient
{
    /// <summary>
    /// Check API server status
    /// </summary>
    /// <returns></returns>
    Task<Ping> GetPingAsync();
}