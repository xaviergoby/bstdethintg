using Hodl.MarketAPI.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hodl.MarketAPI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMarketApis(this IServiceCollection services, IConfiguration config) =>
        services
            .Configure<CoinmarketCapOptions>(config.GetSection("CoinmarketCapOptions"))
            .Configure<CoinGeckoOptions>(config.GetSection("CoinGeckoOptions"))
            .AddScoped<ICryptoMarketApi, CoinGeckoApi>()
            .AddScoped<ICryptoMarketApi, CoinMarketCapApi>()
            .AddScoped<IFiatMarketAPI, ExchangeRatesECB>();
}
