using Hodl.Crypto;
using Hodl.ExplorerAPI.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hodl.ExplorerAPI.Implementations;

public class ArbiScan : EtherScan
{
    private const string ASSET_SYMBOL = "ARB";
    private const string ASSET_NAME = "Arbitrum";
    private const byte ASSET_DECIMALS = 18;

    // The dictionary contains the Explorer URL as key, and the Explorer API
    // URL as value. The explorer url is the domain of the website where the 
    // blockchain explorer is hosted. The API is mostly a subdomain or a
    // specific base path on the address.
    private static readonly IDictionary<string, string> SUPPORTED_URLS = new Dictionary<string, string>
    {
        { "https://arbiscan.io/", "https://api.arbiscan.io" },
        { "https://goerli.arbiscan.io/" , "https://api-goerli.arbiscan.io"}
    };

    protected override string ApiStateConfigName => "Api.State.ArbiScan";

    protected override string ApiMessageTitle => "Block explorer service ArbiScan";

    protected override string ApiMessageContent => "The block explorer data API for ArbiScan recieved an error.\r\r\n{0}";

    protected override string AssetSymbol => ASSET_SYMBOL;
    protected override string AssetName => ASSET_NAME;
    protected override byte AssetDecimals => ASSET_DECIMALS;
    protected override IDictionary<string, string> SupportedUrls => SUPPORTED_URLS;

    public ArbiScan(
        IConfiguration configuration,
        IAppConfigService appConfigService,
        INotificationManager notificationManager,
        ILogger<BscScan> logger
        ) : base(GetOptions(configuration), appConfigService, notificationManager, logger)
    {
    }

    private static EtherScanOptions GetOptions(IConfiguration configuration) => configuration.GetSection("ArbiScanOptions")?.Get<EtherScanOptions>();

    public override bool SupportsAddress(string walletAddress) => WalletFormat.IsArbitrumAddress(walletAddress);
}

