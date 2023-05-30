using Hodl.ExplorerAPI.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Hodl.ExplorerAPI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlockExplorerApis(this IServiceCollection services) =>
        services
            .AddScoped<IBlockExplorer, BscScan>()
            .AddScoped<IBlockExplorer, BtcScan>()
            .AddScoped<IBlockExplorer, EtherScan>()
            .AddScoped<IBlockExplorer, PolygonScan>()
            .AddScoped<IBlockExplorer, ArbiScan>()
            .AddScoped<IBlockExplorer, FtmScan>()
            .AddScoped<IBlockExplorer, AndromedaMetis>()
            .AddScoped<IBlockExplorer, NearBlocks>()
            .AddScoped<IBlockExplorer, Snowtrace>()
            .AddScoped<IBlockExplorer, SubScan>();
}
