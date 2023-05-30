using ExplorerAPI.Utils;
using Hodl.Crypto;
using Hodl.ExplorerAPI.Configurations;
using Hodl.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hodl.ExplorerAPI.Implementations;

public class NearBlocks : ExternalApi, IBlockExplorer, IDisposable
{
    private const string ASSET_SYMBOL = "NEAR";
    private const string ASSET_NAME = "NEAR Protocol";
    private const byte ASSET_DECIMALS = 24;

    private readonly HttpClient _httpClient;
    private readonly NearBlocksOptions _options;

    private static readonly IDictionary<string, string> SUPPORTED_URLS = new Dictionary<string, string>
    {
        // Mainnet (Explorer URL & Endpoint URL)
        { "https://explorer.near.org",  "https://api.nearblocks.io"},
        // Testnet (Explorer URL & Endpoint URL)
        { "https://explorer.testnet.near.org/", "https://api-testnet.nearblocks.io"},
    };

    protected override string ApiStateConfigName => "Api.State.Nearscan";
    protected override string ApiMessageTitle => "Block explorer service Near";
    protected override string ApiMessageContent => "The block explorer data API for Near recieved an error.\r\r\n{0}";

    protected virtual string AssetSymbol => ASSET_SYMBOL;
    protected virtual string AssetName => ASSET_NAME;
    protected virtual byte AssetDecimals => ASSET_DECIMALS;
    protected virtual IDictionary<string, string> SupportedUrls => SUPPORTED_URLS;


    public NearBlocks(
        IConfiguration configuration,
        IAppConfigService appConfigService,
        INotificationManager notificationManager,
        ILogger<NearBlocks> logger
        ) : base(appConfigService, notificationManager, logger)
    {
        _options = GetOptions(configuration);
        _httpClient = new();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options?.ApiKey}");
    }

    private static NearBlocksOptions GetOptions(IConfiguration configuration) => configuration.GetSection("NearBlocksOptions")?.Get<NearBlocksOptions>();

    #region IDisposable implementation
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient.Dispose();
        }
    }
    #endregion

    #region Explorer specific records
    private class AccountResult
    {
        [JsonPropertyName("account")]
        public Account[] Accounts { get; set; }
    }
    private class Account
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("block_hash")]
        public string BlockHash { get; set; }

        [JsonPropertyName("block_height")]
        public long BlockHeight { get; set; }

        [JsonPropertyName("code_hash")]
        public string CodeHash { get; set; }

        [JsonPropertyName("locked")]
        public bool Locked { get; set; }

        [JsonPropertyName("storage_paid_at")]
        public long StoragePaidAt { get; set; }

        [JsonPropertyName("storage_usage")]
        public long StorageUsage { get; set; }

        [JsonPropertyName("account_id")]
        public string AccountId { get; set; }
    }

    private class InventoryResult
    {
        [JsonPropertyName("inventory")]
        public Inventory Inventoy { get; set; }
    }
    private class Inventory
    {
        [JsonPropertyName("fts")]
        public FungableToken[] FungableTokens { get; set; }

        [JsonPropertyName("nfts")]
        public NonFungableToken[] NonFungableTokens { get; set; }
    }
    private class FungableToken
    {
        [JsonPropertyName("contract")]
        public string Contract { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("ft_meta")]
        public FungableTokenMeta FungableTokenMeta { get; set; }

    }
    private class FungableTokenMeta
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("decimals")]
        public int Decimals { get; set; }
    }
    private class NonFungableToken
    {
        [JsonPropertyName("contract")]
        public string Contract { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("base_uri")]
        public string BaseUri { get; set; }

        [JsonPropertyName("reference")]
        public string Reference { get; set; }
    }
    #endregion


    /// <summary>
    /// // Check if this protocol network scanner implementation supports the 3rd party "explorer" given by the url
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public bool SupportsExplorerUrl(string url) => SUPPORTED_URLS.ContainsKey(url);

    /// <summary>
    /// Check if the given account/wallet address is supported (i.e. correct format) by this specific protocol.
    /// </summary>
    /// <param name="walletAddress"></param>
    /// <returns></returns>
    public bool SupportsAddress(string walletAddress) => WalletFormat.IsNearAddress(walletAddress);

    public async Task<IEnumerable<ExplorerBalance>> GetBalances(string explorerUrl, string walletAddress, IEnumerable<TokenContract> tokenContracts, CancellationToken cancelationToken = default)
    {
        if (!SupportsExplorerUrl(explorerUrl))
            throw new NotSupportedException("Explorer url not supported");

        if (!SupportsAddress(walletAddress))
            throw new NotSupportedException("Wallet address not supported");

        var result = new List<ExplorerBalance>();
        var jsonOptions = new JsonSerializerOptions()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
        };

        // First get the wallet balance
        using (var response = await ApiRequestAsync(async () =>
            await _httpClient.GetAsync(WalletBalanceUri(explorerUrl, walletAddress), cancelationToken), cancelationToken))
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStreamAsync(cancelationToken);
                var accountResult = await JsonSerializer.DeserializeAsync<AccountResult>(content, jsonOptions, cancelationToken);

                if (accountResult.Accounts != null && accountResult.Accounts.Length > 0)
                {
                    result.Add(new()
                    {
                        WalletAddress = walletAddress,
                        ExplorerUrl = SupportedUrls[explorerUrl],
                        CurrencySymbol = AssetSymbol,
                        CurrencyName = AssetName,
                        Balance = accountResult.Accounts.First().Amount / (decimal)Math.Pow(10, AssetDecimals),
                        TimeStamp = DateTimeOffset.UtcNow
                    });
                }
            }

        // First get the wallet balance
        using (var response = await ApiRequestAsync(async () =>
            await _httpClient.GetAsync(TokenBalancesUri(explorerUrl, walletAddress), cancelationToken), cancelationToken))
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStreamAsync(cancelationToken);
                var inventoryResult = await JsonSerializer.DeserializeAsync<InventoryResult>(content, jsonOptions, cancelationToken);

                if (inventoryResult.Inventoy.FungableTokens != null)
                {
                    foreach (var ft in inventoryResult.Inventoy.FungableTokens)
                        result.Add(new()
                        {
                            WalletAddress = walletAddress,
                            ExplorerUrl = SupportedUrls[explorerUrl],
                            CurrencySymbol = ft.FungableTokenMeta.Symbol,
                            CurrencyName = ft.FungableTokenMeta.Name,
                            Balance = ft.Amount / (decimal)Math.Pow(10, ft.FungableTokenMeta.Decimals),
                            TimeStamp = DateTimeOffset.UtcNow
                        });
                }
            }

        return result;
    }

    protected virtual Uri WalletBalanceUri(string explorerUrl, string account) =>
        QueryStringService.CreateUrl($"{SupportedUrls[explorerUrl]}/v1/account/{account}", new Dictionary<string, object>());

    protected virtual Uri TokenBalancesUri(string explorerUrl, string account) =>
        QueryStringService.CreateUrl($"{SupportedUrls[explorerUrl]}/v1/account/{account}/inventory", new Dictionary<string, object>());


    public async Task<TransactionModel> GetTransactionInformation(string explorerUrl, string transactionHash, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

}
