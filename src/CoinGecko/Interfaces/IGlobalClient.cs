using CoinGeckoAPI.Entities.Response.Global;

namespace CoinGeckoAPI.Interfaces;

public interface IGlobalClient
{
    /// <summary>
    /// Get cryptocurrency global data
    /// </summary>
    /// <returns></returns>
    Task<Global> GetGlobal();

    /// <summary>
    /// Get cryptocurrency global decentralized finance(defi) data
    /// </summary>
    /// <returns></returns>
    Task<GlobalDeFi> GetGlobalDeFi();
}