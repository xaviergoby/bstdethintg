using Hodl.Crypto;
using Hodl.ExplorerAPI.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hodl.ExplorerAPI.Implementations;

/// <summary>
/// Implements the API for PolygonScan
/// Documentation: https://docs.polygonscan.com/
/// </summary>
public class PolygonScan : EtherScan
{
    private const string ASSET_SYMBOL = "MATIC";
    private const string ASSET_NAME = "Polygon Token";
    private const byte ASSET_DECIMALS = 18;

    private static readonly IDictionary<string, string> SUPPORTED_URLS = new Dictionary<string, string>()
    {
        { "https://polygonscan.com/", "https://api.polygonscan.com" },
        { "https://mumbai.polygonscan.com/", "https://api-testnet.polygonscan.com" }
    };

    protected override string ApiStateConfigName => "Api.State.PolygonScan";

    protected override string ApiMessageTitle => "Block explorer service PolygonScan";

    protected override string ApiMessageContent => "The block explorer data API for PolygonScan recieved an error.\r\r\n{0}";

    protected override string AssetSymbol => ASSET_SYMBOL;
    protected override string AssetName => ASSET_NAME;
    protected override byte AssetDecimals => ASSET_DECIMALS;
    protected override IDictionary<string, string> SupportedUrls => SUPPORTED_URLS;

    public PolygonScan(
        IConfiguration configuration,
        IAppConfigService appConfigService,
        INotificationManager notificationManager,
        ILogger<BscScan> logger
        ) : base(GetOptions(configuration), appConfigService, notificationManager, logger)
    {
    }

    private static EtherScanOptions GetOptions(IConfiguration configuration) => configuration.GetSection("PolygonScanOptions")?.Get<EtherScanOptions>();

    public override bool SupportsAddress(string walletAddress) => WalletFormat.IsPolygonAddress(walletAddress);
}
