using ExplorerAPI.Utils;
using Hodl.Crypto;
using Hodl.ExplorerAPI.Configurations;
using Hodl.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hodl.ExplorerAPI.Implementations;

/// <summary>
/// Implements the API for EtherScan. Documentation: https://docs.etherscan.io/
/// </summary>
public class EtherScan : ExternalApi, IBlockExplorer, IDisposable
{
    private const string ASSET_SYMBOL = "ETH";
    private const string ASSET_NAME = "Ethereum";
    private const byte ASSET_DECIMALS = 18;

    private readonly HttpClient _httpClient;
    private readonly EtherScanOptions _options;

    // The dictionary contains the Explorer URL as key, and the Explorer API
    // URL as value. The explorer url is the domain of the website where the 
    // blockchain explorer is hosted. The API is mostly a subdomain or a
    // specific base path on the address.
    private static readonly IDictionary<string, string> SUPPORTED_URLS = new Dictionary<string, string>()
    {
        { "https://etherscan.io", "https://api.etherscan.io" },
        { "https://goerli.etherscan.io/", "https://api-goerli.etherscan.io" },
        { "https://sepolia.etherscan.io/", "https://api-sepolia.etherscan.io" },
    };

    protected override string ApiStateConfigName => "Api.State.EtherScan";

    protected override string ApiMessageTitle => "Block explorer service EtherScan";

    protected override string ApiMessageContent => "The block explorer data API for EtherScan recieved an error.\r\r\n{0}";

    protected virtual string AssetSymbol => ASSET_SYMBOL;
    protected virtual string AssetName => ASSET_NAME;
    protected virtual byte AssetDecimals => ASSET_DECIMALS;
    protected virtual IDictionary<string, string> SupportedUrls => SUPPORTED_URLS;

    public EtherScan(
        IConfiguration configuration,
        IAppConfigService appConfigService,
        INotificationManager notificationManager,
        ILogger<EtherScan> logger) : this(GetOptions(configuration), appConfigService, notificationManager, logger)
    {
    }

    public EtherScan(
        EtherScanOptions options,
        IAppConfigService appConfigService,
        INotificationManager notificationManager,
        ILogger<EtherScan> logger
        ) : base(appConfigService, notificationManager, logger)
    {
        _options = options;
        _httpClient = new();
    }

    private static EtherScanOptions GetOptions(IConfiguration configuration) => configuration.GetSection("EtherScanOptions")?.Get<EtherScanOptions>();

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
    private record ResponseResult<T>
    {
        [JsonIgnore]
        public bool Success => Status == "1" && Message == "OK";

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("result")]
        public T Result { get; set; }
    }

    private record Result
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

    }

    private record BalanceResult : Result
    {
        [JsonPropertyName("result")]
        public string Balance { get; set; }
    }

    private record class TransactionsResult : Result
    {
        [JsonPropertyName("result")]
        public List<Transaction> Result { get; set; }
    }

    #endregion



    public virtual bool SupportsExplorerUrl(string url) => SupportedUrls.ContainsKey(url);

    public virtual bool SupportsAddress(string walletAddress) => WalletFormat.IsEthereumAddress(walletAddress);

    public async Task<IEnumerable<ExplorerBalance>> GetBalances(string explorerUrl, string walletAddress, IEnumerable<TokenContract> tokenContracts, CancellationToken cancelationToken = default)
    {
        if (!SupportsExplorerUrl(explorerUrl))
            throw new NotSupportedException("Explorer url not supported");

        if (!SupportsAddress(walletAddress))
            throw new NotSupportedException("Wallet address not supported");

        // Example: https://api.etherscan.com/api?module=account&action=balance&address=0xde0b295669a9fd93d5f28d9ec85e40f4cb697bae&tag=latest&apikey=ZHVZE4TXKZHZ6A82KCM5TT559PUY2NFN8P

        var result = new List<ExplorerBalance>();

        // First get the wallet balance
        using (var response = await ApiRequestAsync(async () =>
            await _httpClient.GetAsync(WalletBalanceUri(explorerUrl, walletAddress), cancelationToken), cancelationToken))
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStreamAsync(cancelationToken);
                var balanceResult = await JsonSerializer.DeserializeAsync<BalanceResult>(content, cancellationToken: cancelationToken);

                if (balanceResult.Status.Equals("1") &&
                    decimal.TryParse(balanceResult.Balance, out decimal balance))
                {
                    result.Add(new()
                    {
                        WalletAddress = walletAddress,
                        ExplorerUrl = SupportedUrls[explorerUrl],
                        CurrencySymbol = AssetSymbol,
                        CurrencyName = AssetName,
                        Balance = balance / (decimal)Math.Pow(10, AssetDecimals),
                        TimeStamp = DateTimeOffset.UtcNow
                    });
                }
            }

        // Then get the balances for the registered tokens
        foreach (var tokenContract in tokenContracts)
        {
            using var response = await ApiRequestAsync(async () =>
                await _httpClient.GetAsync(TokenBalanceUri(explorerUrl, walletAddress, tokenContract.Address), cancelationToken), cancelationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStreamAsync(cancelationToken);
                var balanceResult = await JsonSerializer.DeserializeAsync<BalanceResult>(content, cancellationToken: cancelationToken);

                if (balanceResult.Status.Equals("1") &&
                    decimal.TryParse(balanceResult.Balance, out decimal balance))
                {
                    result.Add(new()
                    {
                        WalletAddress = walletAddress,
                        ExplorerUrl = SupportedUrls[explorerUrl],
                        CurrencySymbol = tokenContract.TokenSymbol,
                        CurrencyName = tokenContract.TokenName,
                        Balance = balance / (decimal)Math.Pow(10, tokenContract.Decimals),
                        TimeStamp = DateTimeOffset.UtcNow
                    });
                }
            }
        }

        return result;
    }

    protected virtual Uri WalletBalanceUri(string explorerUrl, string address) =>
        QueryStringService.CreateUrl($"{SupportedUrls[explorerUrl]}/api", new Dictionary<string, object>
        {
            { "module", "account" },
            { "action", "balance" },
            { "address", address },
            { "tag", "latest" },
            { "apikey", _options?.ApiKey }
        });

    protected virtual Uri TokenBalanceUri(string explorerUrl, string address, string tokenAddress) =>
        QueryStringService.CreateUrl($"{SupportedUrls[explorerUrl]}/api", new Dictionary<string, object>
        {
            { "module", "account" },
            { "action", "tokenbalance" },
            { "contractaddress", tokenAddress },
            { "address", address },
            { "tag", "latest" },
            { "apikey", _options?.ApiKey }
        });

    /// <summary>
    /// Doc source: https://docs.etherscan.io/api-endpoints/accounts#get-a-list-of-normal-transactions-by-address
    /// </summary>
    /// <param name="explorerUrl"></param>
    /// <param name="walletAddress"></param>
    /// <param name="startBlock"></param>
    /// <param name="endBlock"></param>
    /// <param name="page"></param>
    /// <param name="offset"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    private Uri NormalTransactionsByAddressUri(string explorerUrl, string walletAddress, string startBlock = "0", string endBlock = "99999999", string page = "1", string offset = "10", string sort = "asc")
    {
        return QueryStringService.CreateUrl($"{SUPPORTED_URLS[explorerUrl]}/api", new Dictionary<string, object>
        {
                    { "module", "account" },
                    { "action", "txlist" },
                    { "address ", walletAddress },
                    { "startBlock", startBlock },
                    { "endBlock", endBlock },
                    { "page", page },
                    { "offset", offset},
                    { "sort", sort},
                    { "apikey", _options?.ApiKey }
        });
    }

    /// <summary>
    /// This method is for retrieving information about a transaction given a specific hash address.
    /// Note this method can retrieve information about any transaction, regardless of if it has internal transactions or not.
    /// However, also note that the information on pontential international transactions will not be retruned.
    /// </summary>
    /// <param name="explorerUrl"></param>
    /// <param name="transactionHash"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public async Task<IEnumerable<Transaction>> NormalTransactionsByAddress(string explorerUrl, string walletAddress, string startBlock = "0", string endBlock = "99999999", string page = "1", string offset = "10", string sort = "asc", CancellationToken cancellationToken = default)
    {
        if (!SupportsExplorerUrl(explorerUrl))
            throw new NotSupportedException("Explorer url not supported");

        if (!SupportsAddress(walletAddress))
            throw new NotSupportedException("Wallet address not supported");

        var normalTransactions = new List<Transaction>();

        using (var response = await ApiRequestAsync(async () =>
        await _httpClient.GetAsync(NormalTransactionsByAddressUri(explorerUrl, walletAddress), cancellationToken), cancellationToken))
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStreamAsync(cancellationToken);
                var transactionResult = await JsonSerializer.DeserializeAsync<TransactionsResult>(content, cancellationToken: cancellationToken);

                if (transactionResult.Status.Equals("1"))
                {
                    normalTransactions = transactionResult.Result;
                }
                else
                {
                    throw new Exception($"No results normal transaction results retrieved for given address (Status = {transactionResult.Status}");
                }
            }
            else
            {
                throw new Exception($"API Exception");
            }

        return normalTransactions;
    }

    /// <summary>
    /// Doc source: https://docs.etherscan.io/api-endpoints/accounts#get-a-list-of-internal-transactions-by-address
    /// </summary>
    /// <param name="explorerUrl"></param>
    /// <param name="walletAddress"></param>
    /// <param name="startBlock"></param>
    /// <param name="endBlock"></param>
    /// <param name="page"></param>
    /// <param name="offset"></param>
    /// <param name="sort"></param>
    /// <returns></returns>
    private Uri InternalTransactionsByAddressUri(string explorerUrl, string walletAddress, string startBlock = "0", string endBlock = "99999999", string page = "1", string offset = "10", string sort = "asc")
    {
        return QueryStringService.CreateUrl($"{SUPPORTED_URLS[explorerUrl]}/api", new Dictionary<string, object>
        {
                    { "module", "account" },
                    { "action", "txlistinternal" },
                    { "address ", walletAddress },
                    { "startBlock", startBlock },
                    { "endBlock", endBlock },
                    { "page", page },
                    { "offset", offset},
                    { "sort", sort},
                    { "apikey", _options?.ApiKey }
        });
    }

    public async Task<IEnumerable<Transaction>> InternalTransactionsByAddress(string explorerUrl, string walletAddress, string startBlock = "0", string endBlock = "99999999", string page = "1", string offset = "10", string sort = "asc", CancellationToken cancellationToken = default)
    {
        if (!SupportsExplorerUrl(explorerUrl))
            throw new NotSupportedException("Explorer url not supported");

        if (!SupportsAddress(walletAddress))
            throw new NotSupportedException("Wallet address not supported");

        var internalTransactions = new List<Transaction>();

        using (var response = await ApiRequestAsync(async () =>
        await _httpClient.GetAsync(InternalTransactionsByAddressUri(explorerUrl, walletAddress), cancellationToken), cancellationToken))
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStreamAsync(cancellationToken);
                var transactionResult = await JsonSerializer.DeserializeAsync<TransactionsResult>(content, cancellationToken: cancellationToken);

                if (transactionResult.Status.Equals("1"))
                {
                    internalTransactions = transactionResult.Result;
                }
                else
                {
                    throw new Exception($"No results normal internal results retrieved for given address(Status = {transactionResult.Status}");
                }
            }
            else
            {
                throw new Exception($"API Exception");
            }

        return internalTransactions;
    }

    /// <summary>
    /// Doc source: https://docs.etherscan.io/api-endpoints/accounts#get-internal-transactions-by-transaction-hash
    /// </summary>
    /// <param name="explorerUrl"></param>
    /// <param name="txhash"></param>
    /// <returns></returns>
    private Uri InternalTransactionsByHashUri(string explorerUrl, string txhash)
    {
        return QueryStringService.CreateUrl($"{SUPPORTED_URLS[explorerUrl]}/api", new Dictionary<string, object>
        {
                    { "module", "account" },
                    { "action", "txlistinternal" },
                    { "txhash ", txhash },
                    { "apikey", _options?.ApiKey }
        });
    }

    public async Task<IEnumerable<Transaction>> InternalTransactionsByHash(string explorerUrl, string txhash, CancellationToken cancellationToken = default)
    {
        if (!SupportsExplorerUrl(explorerUrl))
            throw new NotSupportedException("Explorer url not supported");

        // Use the txHash to get the ?from?/?to? wallet address and then use this to check if SupportsAddress(walletAddress) is true
        //if (!SupportsAddress(walletAddress))
            //throw new NotSupportedException("Wallet address not supported");

        var internalTransactions = new List<Transaction>();

        using (var response = await ApiRequestAsync(async () =>
        await _httpClient.GetAsync(InternalTransactionsByHashUri(explorerUrl, txhash), cancellationToken), cancellationToken))
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStreamAsync(cancellationToken);
                var transactionResult = await JsonSerializer.DeserializeAsync<TransactionsResult>(content, cancellationToken: cancellationToken);

                if (transactionResult.Status.Equals("1"))
                {
                    internalTransactions = transactionResult.Result;
                }
                else
                {
                    throw new Exception($"No results normal internal result retrieved for given hash(Status = {transactionResult.Status}");
                }
            }
            else
            {
                throw new Exception($"API Exception");
            }

        return internalTransactions;
    }

    // TO-DO REMINDER: Implement the func to also get a transaction by its corresponding hash.
    // https://docs.etherscan.io/api-endpoints/geth-parity-proxy#eth_gettransactionbyhash

}
