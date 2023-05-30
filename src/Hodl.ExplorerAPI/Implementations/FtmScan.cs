using Hodl.Crypto;
using Hodl.ExplorerAPI.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hodl.ExplorerAPI.Implementations;

/// <summary>
/// Implements the API for FtmScan
/// Documentation: https://docs.ftmscan.com/
/// </summary>
public class FtmScan : EtherScan
{
    private const string ASSET_SYMBOL = "FTM";
    private const string ASSET_NAME = "Fantom Token";
    private const byte ASSET_DECIMALS = 18;

    // The dictionary contains the Explorer URL as key, and the Explorer API
    // URL as value. The explorer url is the domain of the website where the 
    // blockchain explorer is hosted. The API is mostly a subdomain or a
    // specific base path on the address.
    private static readonly IDictionary<string, string> SUPPORTED_URLS = new Dictionary<string, string>()
    {
        { "https://ftmscan.com/", "https://api.ftmscan.com/" },
        { "https://testnet.ftmscan.com/", "https://api-testnet.ftmscan.com/" }
    };

    protected override string ApiStateConfigName => "Api.State.FtmScan";

    protected override string ApiMessageTitle => "Block explorer service FtmScan";

    protected override string ApiMessageContent => "The block explorer data API for FtmScan recieved an error.\r\r\n{0}";

    protected override string AssetSymbol => ASSET_SYMBOL;
    protected override string AssetName => ASSET_NAME;
    protected override byte AssetDecimals => ASSET_DECIMALS;
    protected override IDictionary<string, string> SupportedUrls => SUPPORTED_URLS;

    public FtmScan(
        IConfiguration configuration,
        IAppConfigService appConfigService,
        INotificationManager notificationManager,
        ILogger<FtmScan> logger
        ) : base(GetOptions(configuration), appConfigService, notificationManager, logger)
    {
    }

    private static EtherScanOptions GetOptions(IConfiguration configuration) => configuration.GetSection("FtmScanOptions")?.Get<EtherScanOptions>();

    public override bool SupportsAddress(string walletAddress) => WalletFormat.IsFantomAddress(walletAddress);
}
