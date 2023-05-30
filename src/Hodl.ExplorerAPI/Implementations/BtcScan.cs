using Hodl.Crypto;
using Hodl.Framework;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hodl.ExplorerAPI.Implementations;

public class BtcScan : ExternalApi, IBlockExplorer
{
    private const string ASSET_SYMBOL = "BTC";
    private const string ASSET_NAME = "Bitcoin";
    private const byte ASSET_DECIMALS = 8;

    private readonly HttpClient _httpClient;

    // The dictionary contains the Explorer URL as key, and the Explorer API
    // URL as value. The explorer url is the domain of the website where the 
    // blockchain explorer is hosted. The API is mostly a subdomain or a
    // specific base path on the address.
    private static readonly IDictionary<string, string> SUPPORTED_URLS = new Dictionary<string, string>()
    {
        { "https://btcscan.org", "https://btcscan.org/api" }
    };


    public BtcScan(
        IAppConfigService appConfigService,
        INotificationManager notificationManager,
        ILogger<ExternalApi> logger) :
            base(appConfigService, notificationManager, logger)
    {
        _httpClient = new();
    }

    protected override string ApiStateConfigName => "Api.State.BtcScan";

    protected override string ApiMessageTitle => "Block explorer service BtcScan";

    protected override string ApiMessageContent => "The block explorer data API for BtcScan recieved an error.\r\r\n{0}";


    #region Explorer specific records
    private record ChainStats
    {
        [JsonPropertyName("funded_txo_count")]
        public decimal FundedTxoCount { get; set; }

        [JsonPropertyName("funded_txo_sum")]
        public decimal FundedTxoSum { get; set; }

        [JsonPropertyName("spent_txo_count")]
        public decimal SpentTxoCount { get; set; }

        [JsonPropertyName("spent_txo_sum")]
        public decimal SpentTxoSum { get; set; }

        [JsonPropertyName("tx_count")]
        public decimal TxCount { get; set; }
    }

    private record Wallet
    {
        public string Address { get; set; }

        [JsonPropertyName("chain_stats")]
        public ChainStats ChainStats { get; set; }

        [JsonPropertyName("mempool_stats")]
        public ChainStats MempoolStats { get; set; }
    }
    #endregion

    public bool SupportsExplorerUrl(string url) => SUPPORTED_URLS.ContainsKey(url);

    public bool SupportsAddress(string walletAddress) => WalletFormat.IsBitcoinAddress(walletAddress);

    public async Task<IEnumerable<ExplorerBalance>> GetBalances(string explorerUrl, string walletAddress, IEnumerable<TokenContract> tokenContracts, CancellationToken cancelationToken = default)
    {
        if (!SupportsExplorerUrl(explorerUrl))
            throw new NotSupportedException("Explorer url not supported");

        if (!SupportsAddress(walletAddress))
            throw new NotSupportedException("Wallet address not supported");

        // Example: https://btcscan.org/api/address/bc1qxy2kgdygjrsqtzq2n0yrf2493p83kkfjhx0wlh

        var result = new List<ExplorerBalance>();

        using (var response = await ApiRequestAsync(async () =>
            await _httpClient.GetAsync($"{SUPPORTED_URLS[explorerUrl]}/address/{walletAddress}", cancelationToken), cancelationToken))

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStreamAsync(cancelationToken);
                var wallet = await JsonSerializer.DeserializeAsync<Wallet>(content, cancellationToken: cancelationToken);

                if (wallet?.ChainStats != null)
                {
                    result.Add(new()
                    {
                        WalletAddress = walletAddress,
                        ExplorerUrl = explorerUrl,
                        CurrencySymbol = ASSET_SYMBOL,
                        CurrencyName = ASSET_NAME,
                        Balance = (decimal)(wallet.ChainStats.FundedTxoCount - wallet.ChainStats.SpentTxoSum) / (decimal)Math.Pow(10, ASSET_DECIMALS),
                        TimeStamp = DateTimeOffset.UtcNow
                    });
                }
            }

        return result;
    }

    public async Task<TransactionModel> GetTransactionInformation(string explorerUrl, string transactionHash, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
