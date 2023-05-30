using Hodl.Crypto;
using Hodl.ExplorerAPI.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hodl.ExplorerAPI.Implementations;

/// <summary>
/// Implements the API for EtherScan. Documentation: https://docs.snowtrace.io/getting-started/endpoint-urls
/// </summary>
public class Snowtrace : EtherScan
{
    private const string ASSET_SYMBOL = "AVAX";
    private const string ASSET_NAME = "Avalanche";
    private const byte ASSET_DECIMALS = 18;

    // The dictionary contains the Explorer URL as key, and the Explorer API
    // URL as value. The explorer url is the domain of the website where the 
    // blockchain explorer is hosted. The API is mostly a subdomain or a
    // specific base path on the address.
    private static readonly IDictionary<string, string> SUPPORTED_URLS = new Dictionary<string, string>()
    {
        { "https://snowtrace.io/", "https://api.snowtrace.io" },
        { "https://testnet.snowtrace.io/", "https://api-testnet.snowtrace.io" },
    };

    protected override string ApiStateConfigName => "Api.State.Snowtrace";

    protected override string ApiMessageTitle => "Block explorer service Snowtrace ";

    protected override string ApiMessageContent => "The block explorer data API for Snowtrace recieved an error.\r\r\n{0}";

    protected override string AssetSymbol => ASSET_SYMBOL;
    protected override string AssetName => ASSET_NAME;
    protected override byte AssetDecimals => ASSET_DECIMALS;
    protected override IDictionary<string, string> SupportedUrls => SUPPORTED_URLS;

    public Snowtrace(
        IConfiguration configuration,
        IAppConfigService appConfigService,
        INotificationManager notificationManager,
        ILogger<BscScan> logger
        ) : base(GetOptions(configuration), appConfigService, notificationManager, logger)
    {
    }

    private static EtherScanOptions GetOptions(IConfiguration configuration) => configuration.GetSection("SnowtraceOptions")?.Get<EtherScanOptions>();

    public override bool SupportsAddress(string walletAddress) => WalletFormat.IsAvaxAddress(walletAddress);
}
