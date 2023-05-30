using Hodl.Crypto;
using Hodl.ExplorerAPI.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hodl.ExplorerAPI.Implementations;

/// <summary>
/// Implements the API for BscScan. Documentation: https://docs.bscscan.com/
/// </summary>
public class BscScan : EtherScan
{
    private const string ASSET_SYMBOL = "BNB";
    private const string ASSET_NAME = "Binance Smart Chain";
    private const byte ASSET_DECIMALS = 18;

    // The dictionary contains the Explorer URL as key, and the Explorer API
    // URL as value. The explorer url is the domain of the website where the 
    // blockchain explorer is hosted. The API is mostly a subdomain or a
    // specific base path on the address.
    private static readonly IDictionary<string, string> SUPPORTED_URLS = new Dictionary<string, string>()
    {
        { "https://bscscan.com", "https://api.bscscan.com" },
        { "https://testnet.bscscan.com", "https://api-testnet.bscscan.com" }
    };

    protected override string ApiStateConfigName => "Api.State.BscScan";

    protected override string ApiMessageTitle => "Block explorer service BscScan";

    protected override string ApiMessageContent => "The block explorer data API for BscScan recieved an error.\r\r\n{0}";

    protected override string AssetSymbol => ASSET_SYMBOL;
    protected override string AssetName => ASSET_NAME;
    protected override byte AssetDecimals => ASSET_DECIMALS;
    protected override IDictionary<string, string> SupportedUrls => SUPPORTED_URLS;

    public BscScan(
        IConfiguration configuration,
        IAppConfigService appConfigService,
        INotificationManager notificationManager,
        ILogger<BscScan> logger
        ) : base(GetOptions(configuration), appConfigService, notificationManager, logger)
    {
    }

    private static EtherScanOptions GetOptions(IConfiguration configuration) => configuration.GetSection("BscScanOptions")?.Get<EtherScanOptions>();

    public override bool SupportsAddress(string walletAddress) => WalletFormat.IsBnbBep20Address(walletAddress);
}
