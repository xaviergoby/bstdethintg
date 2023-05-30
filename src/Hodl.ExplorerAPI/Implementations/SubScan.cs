using Hodl.Crypto;
using Hodl.ExplorerAPI.Configurations;
using Hodl.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hodl.ExplorerAPI.Implementations;

public class SubScan : ExternalApi, IBlockExplorer, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly SubScanOptions _options;

    // The dictionary contains the Explorer URL as key, and the Explorer API
    // endpoint as value. The explorer url is the domain of the website where
    // the blockchain explorer is hosted. The API is mostly a subdomain or a
    // specific base path on the address.
    // For all supported explorers check: https://support.subscan.io/#api-endpoints
    static readonly IDictionary<string, string> EXPLORER_API_ENDPOINTS = new Dictionary<string, string>
    {
        { "https://polkadot.subscan.io/",  "https://polkadot.api.subscan.io"},
        { "https://kusama.subscan.io/",  "https://kusama.api.subscan.io"},
        { "https://darwinia.subscan.io/",  "https://darwinia.api.subscan.io"},
        { "https://acala.subscan.io/",  "https://acala.api.subscan.io"},
        { "https://acala-testnet.subscan.io/",  "https://acala-testnet.api.subscan.io"},
        { "https://alephzero.subscan.io/",  "https://alephzero.api.subscan.io"},
        { "https://altair.subscan.io/",  "https://altair.api.subscan.io"},
        { "https://astar.subscan.io/",  "https://astar.api.subscan.io"},
        { "https://bajun.subscan.io/",  "https://bajun.api.subscan.io"},
        { "https://basilisk.subscan.io/",  "https://basilisk.api.subscan.io"},
        { "https://bifrost.subscan.io/",  "https://bifrost.api.subscan.io"},
        { "https://bifrost-kusama.subscan.io/",  "https://bifrost-kusama.api.subscan.io"},
        { "https://bifrost-testnet.subscan.io/",  "https://bifrost-testnet.api.subscan.io"},
        { "https://calamari.subscan.io/",  "https://calamari.api.subscan.io"},
        { "https://centrifuge.subscan.io/",  "https://centrifuge.api.subscan.io"},
        { "https://centrifuge-standalone-history.subscan.io/",  "https://centrifuge-standalone-history.api.subscan.io"},
        { "https://chainx.subscan.io/",  "https://chainx.api.subscan.io"},
        { "https://clover.subscan.io/",  "https://clover.api.subscan.io"},
        { "https://clv.subscan.io/",  "https://clv.api.subscan.io"},
        { "https://clover-testnet.subscan.io/",  "https://clover-testnet.api.subscan.io"},
        { "https://composable.subscan.io/",  "https://composable.api.subscan.io"},
        { "https://crab.subscan.io/",  "https://crab.api.subscan.io"},
        { "https://crust.subscan.io/",  "https://crust.api.subscan.io"},
        { "https://maxwell.subscan.io/",  "https://maxwell.api.subscan.io"},
        { "https://shadow.subscan.io/",  "https://shadow.api.subscan.io"},
        { "https://dali.subscan.io/",  "https://dali.api.subscan.io"},
        { "https://darwinia-parachain.subscan.io/",  "https://darwinia-parachain.api.subscan.io"},
        { "https://dbc.subscan.io/",  "https://dbc.api.subscan.io"},
        { "https://dock.subscan.io/",  "https://dock.api.subscan.io"},
        { "https://dolphin.subscan.io/",  "https://dolphin.api.subscan.io"},
        { "https://edgeware.subscan.io/",  "https://edgeware.api.subscan.io"},
        { "https://efinity.subscan.io/",  "https://efinity.api.subscan.io"},
        { "https://encointer.subscan.io/",  "https://encointer.api.subscan.io"},
        { "https://equilibrium.subscan.io/",  "https://equilibrium.api.subscan.io"},
        { "https://genshiro.subscan.io/",  "https://genshiro.api.subscan.io"},
        { "https://humanode.subscan.io/",  "https://humanode.api.subscan.io"},
        { "https://hydradx.subscan.io/",  "https://hydradx.api.subscan.io"},
        { "https://integritee.subscan.io/",  "https://integritee.api.subscan.io"},
        { "https://interlay.subscan.io/",  "https://interlay.api.subscan.io"},
        { "https://karura.subscan.io/",  "https://karura.api.subscan.io"},
        { "https://kintsugi.subscan.io/",  "https://kintsugi.api.subscan.io"},
        { "https://khala.subscan.io/",  "https://khala.api.subscan.io"},
        { "https://kilt-testnet.subscan.io/",  "https://kilt-testnet.api.subscan.io"},
        { "https://spiritnet.subscan.io/",  "https://spiritnet.api.subscan.io"},
        { "https://litmus.subscan.io/",  "https://litmus.api.subscan.io"},
        { "https://mangatax.subscan.io/",  "https://mangatax.api.subscan.io"},
        { "https://moonbase.subscan.io/",  "https://moonbase.api.subscan.io"},
        { "https://moonbeam.subscan.io/",  "https://moonbeam.api.subscan.io"},
        { "https://moonriver.subscan.io/",  "https://moonriver.api.subscan.io"},
        { "https://nodle.subscan.io/",  "https://nodle.api.subscan.io"},
        { "https://origintrail.subscan.io/",  "https://origintrail.api.subscan.io"},
        { "https://origintrail-testnet.subscan.io/",  "https://origintrail-testnet.api.subscan.io"},
        { "https://pangolin.subscan.io/",  "https://pangolin.api.subscan.io"},
        { "https://pangolin-parachain.subscan.io/",  "https://pangolin-parachain.api.subscan.io"},
        { "https://pangoro.subscan.io/",  "https://pangoro.api.subscan.io"},
        { "https://parallel.subscan.io/",  "https://parallel.api.subscan.io"},
        { "https://parallel-heiko.subscan.io/",  "https://parallel-heiko.api.subscan.io"},
        { "https://phala.subscan.io/",  "https://phala.api.subscan.io"},
        { "https://picasso.subscan.io/",  "https://picasso.api.subscan.io"},
        { "https://pioneer.subscan.io/",  "https://pioneer.api.subscan.io"},
        { "https://polkadex.subscan.io/",  "https://polkadex.api.subscan.io"},
        { "https://polymesh.subscan.io/",  "https://polymesh.api.subscan.io"},
        { "https://polymesh-testnet.subscan.io/",  "https://polymesh-testnet.api.subscan.io"},
        { "https://plasm.subscan.io/",  "https://plasm.api.subscan.io"},
        { "https://quartz.subscan.io/",  "https://quartz.api.subscan.io"},
        { "https://reef.subscan.io/",  "https://reef.api.subscan.io"},
        { "https://robonomics.subscan.io/",  "https://robonomics.api.subscan.io"},
        { "https://rockmine.subscan.io/",  "https://rockmine.api.subscan.io"},
        { "https://rococo.subscan.io/",  "https://rococo.api.subscan.io"},
        { "https://sakura.subscan.io/",  "https://sakura.api.subscan.io"},
        { "https://shibuya.subscan.io/",  "https://shibuya.api.subscan.io"},
        { "https://shiden.subscan.io/",  "https://shiden.api.subscan.io"},
        { "https://snow.subscan.io/",  "https://snow.api.subscan.io"},
        { "https://sora.subscan.io/",  "https://sora.api.subscan.io"},
        { "https://subspace.subscan.io/",  "https://subspace.api.subscan.io"},
        { "https://stafi.subscan.io/",  "https://stafi.api.subscan.io"},
        { "https://statemine.subscan.io/",  "https://statemine.api.subscan.io"},
        { "https://statemint.subscan.io/",  "https://statemint.api.subscan.io"},
        { "https://datahighway.subscan.io/",  "https://datahighway.api.subscan.io"},
        { "https://turing.subscan.io/",  "https://turing.api.subscan.io"},
        { "https://unique.subscan.io/",  "https://unique.api.subscan.io"},
        { "https://westend.subscan.io/",  "https://westend.api.subscan.io"},
        { "https://zeitgeist.subscan.io/",  "https://zeitgeist.api.subscan.io"},
    };

    protected override string ApiStateConfigName => "Api.State.SubScan";

    protected override string ApiMessageTitle => "Block explorer service SubScan";

    protected override string ApiMessageContent => "The block explorer data API for SubScan recieved an error.\r\r\n{0}";

    public SubScan(
        IConfiguration configuration,
        IAppConfigService appConfigService,
        INotificationManager notificationManager,
        ILogger<SubScan> logger
        ) : base(appConfigService, notificationManager, logger)
    {
        _options = GetOptions(configuration);
        _httpClient = new();
    }

    private static SubScanOptions GetOptions(IConfiguration configuration) => configuration.GetSection("SubScanOptions")?.Get<SubScanOptions>();

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

    #region Explorer specific results
    private class AccountTokensResult
    {
        [JsonPropertyName("code")]
        public long Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("generated_at")]
        public long GeneratedAt { get; set; }

        [JsonPropertyName("data")]
        public AccountTokensData Data { get; set; }
    };



    private class AccountTokensData
    {
        [JsonPropertyName("native")]
        public TokensBalanceNative[] Native { get; set; }
        [JsonPropertyName("builtin")]
        public TokensBalance[] BuiltIn { get; set; }
        [JsonPropertyName("assets")]
        public TokensBalance[] Assets { get; set; }
        [JsonPropertyName("ERC20")]
        public TokensBalance[] Erc20 { get; set; }
    };

    private class TokensBalance
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("unique_id")]
        public string UniqueId { get; set; }

        [JsonPropertyName("decimals")]
        public int Decimals { get; set; }

        [JsonPropertyName("balance")]
        public decimal Balance { get; set; }
    }

    private class TokensBalanceNative : TokensBalance
    {
        [JsonPropertyName("lock")]
        public int Lock { get; set; }

        [JsonPropertyName("reserved")]
        public decimal Reserved { get; set; }

        [JsonPropertyName("bonded")]
        public decimal Bonded { get; set; }

        [JsonPropertyName("unbonding")]
        public decimal Unbonding { get; set; }

        [JsonPropertyName("democracy_lock")]
        public decimal DemocracyLock { get; set; }

        [JsonPropertyName("election_lock")]
        public decimal ElectionLock { get; set; }
    };
    #endregion

    public bool SupportsExplorerUrl(string url) => EXPLORER_API_ENDPOINTS.ContainsKey(url);

    public bool SupportsAddress(string walletAddress) => WalletFormat.IsAlephZeroAddress(walletAddress);

    /// <summary>
    /// Base URL = https://alephzero.api.subscan.io/
    /// Path: api/scan/account/
    /// </summary>
    /// <param name="explorerUrl"></param>
    /// <returns></returns>
    private static Uri WalletBalancesUri(string explorerUrl)
    {
        return new Uri($"{EXPLORER_API_ENDPOINTS[explorerUrl]}/api/scan/account/tokens");
    }

    public async Task<IEnumerable<ExplorerBalance>> GetBalances(string explorerUrl, string walletAddress, IEnumerable<TokenContract> tokenContracts, CancellationToken cancelationToken = default)
    {
        if (!SupportsExplorerUrl(explorerUrl))
            throw new NotSupportedException("Explorer url not supported");

        if (!SupportsAddress(walletAddress))
            throw new NotSupportedException("Wallet address not supported");


        var result = new List<ExplorerBalance>();

        var request = new HttpRequestMessage()
        {
            Method = HttpMethod.Post,
            RequestUri = WalletBalancesUri(explorerUrl),
            Headers = {
                { HttpRequestHeader.ContentType.ToString(), "application/JSON" },
                { "X-API-Key", _options.ApiKey }
            },
            Content = new StringContent(JsonSerializer.Serialize(new Dictionary<string, string> { { "address", walletAddress } }), Encoding.UTF8)
        };

        // Get the wallet balances
        using (var response = await ApiRequestAsync(async () =>
            await _httpClient.SendAsync(request, cancelationToken), cancelationToken))
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStreamAsync(cancelationToken);
                var options = new JsonSerializerOptions()
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
                };
                var balanceResult = await JsonSerializer.DeserializeAsync<AccountTokensResult>(responseContent, options, cancelationToken);

                // For Subscan the "Code" you want to have for a succesful request is 0 (as opposed to 0)
                if (balanceResult.Code.Equals(0))
                {
                    AddBalances(balanceResult.Data.Native);
                    AddBalances(balanceResult.Data.BuiltIn);
                    AddBalances(balanceResult.Data.Assets);
                    AddBalances(balanceResult.Data.Erc20);
                }
            }

        return result;

        void AddBalances(IEnumerable<TokensBalance> balances)
        {
            if (balances == null) return;

            foreach (var balance in balances)
            {
                result.Add(new()
                {
                    WalletAddress = walletAddress,
                    ExplorerUrl = EXPLORER_API_ENDPOINTS[explorerUrl],
                    CurrencySymbol = balance.Symbol,
                    CurrencyName = string.Empty,
                    Balance = balance.Balance / (decimal)Math.Pow(10, balance.Decimals),
                    TimeStamp = DateTimeOffset.UtcNow
                });
            }
        }
    }


    public async Task<TransactionModel> GetTransactionInformation(string explorerUrl, string transactionHash, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }


}
