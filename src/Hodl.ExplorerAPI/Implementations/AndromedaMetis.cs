using ExplorerAPI.Utils;
using Hodl.Crypto;
using Hodl.ExplorerAPI.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hodl.ExplorerAPI.Implementations;

/// <summary>
/// Implements the API for Andromeda Explorer Metis Documentation: https://andromeda-explorer.metis.io/api-docs
/// Metis Connection Details: https://docs.metis.io/dev/get-started/metis-connection-details
/// The Mainnet is what is called "Andromeda" and the testnet is called "Goerli". 
/// </summary>
public class AndromedaMetis : EtherScan
{
    private const string ASSET_SYMBOL = "METIS";
    private const string ASSET_NAME = "MetisDAO";
    private const byte ASSET_DECIMALS = 18;

    // The dictionary contains the Explorer URL as key, and the Explorer API
    // URL as value. The explorer url is the domain of the website where the 
    // blockchain explorer is hosted. The API is mostly a subdomain or a
    // specific base path on the address.
    private static readonly IDictionary<string, string> SUPPORTED_URLS = new Dictionary<string, string>()
    {
        { "https://andromeda-explorer.metis.io/", "https://andromeda-explorer.metis.io" },
        { "https://goerli.explorer.metisdevops.link/", "https://goerli.explorer.metisdevops.link" },
    };

    protected override string ApiStateConfigName => "Api.State.AndromedaExplorerMetis";

    protected override string ApiMessageTitle => "Block explorer service AndromedaExplorerMetis";

    protected override string ApiMessageContent => "The block explorer data API for AndromedaExplorerMetis recieved an error.\r\r\n{0}";

    protected override string AssetSymbol => ASSET_SYMBOL;
    protected override string AssetName => ASSET_NAME;
    protected override byte AssetDecimals => ASSET_DECIMALS;
    protected override IDictionary<string, string> SupportedUrls => SUPPORTED_URLS;

    public AndromedaMetis(
        IConfiguration configuration,
        IAppConfigService appConfigService,
        INotificationManager notificationManager,
        ILogger<BscScan> logger
        ) : base(GetOptions(configuration), appConfigService, notificationManager, logger)
    {
    }

    private static EtherScanOptions GetOptions(IConfiguration configuration) => configuration.GetSection("AndromedaMetisOptions")?.Get<EtherScanOptions>();

    public override bool SupportsAddress(string walletAddress) => WalletFormat.IsMetisAddress(walletAddress);

    protected override Uri WalletBalanceUri(string explorerUrl, string address)
    {
        return QueryStringService.CreateUrl($"{SupportedUrls[explorerUrl]}/api", new Dictionary<string, object>
        {
            { "module", "account" },
            { "action", "balance" },
            { "address", address }
        });
    }

    protected override Uri TokenBalanceUri(string explorerUrl, string address, string tokenAddress)
    {
        return QueryStringService.CreateUrl($"{SupportedUrls[explorerUrl]}/api", new Dictionary<string, object>
        {
            { "module", "account" },
            { "action", "tokenbalance" },
            { "contractaddress", tokenAddress },
            { "address", address }
        });
    }
}
