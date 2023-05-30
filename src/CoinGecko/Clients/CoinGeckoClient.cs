using CoinGeckoAPI.Interfaces;
using CoinGeckoAPI.JSONConverters;
using System.Text.Json;

namespace CoinGeckoAPI.Clients;

public partial class CoinGeckoClient : IDisposable, ICoinGeckoClient
{
    private static readonly Lazy<CoinGeckoClient> Lazy = new(() => new CoinGeckoClient());

    #region Fields

    private readonly HttpClient _httpClient;
    private bool _isDisposed;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly string _apiKey;

    #endregion Fields

    #region Constructors

    public CoinGeckoClient() : this((JsonSerializerOptions)null)
    {
    }

    public CoinGeckoClient(string apiKey) : this((JsonSerializerOptions)null, apiKey)
    {
    }


    public CoinGeckoClient(HttpClientHandler httpClientHandler) : this(httpClientHandler, serializerOptions: null)
    {
    }

    public CoinGeckoClient(HttpClientHandler httpClientHandler, string apiKey) : this(httpClientHandler, null, apiKey)
    {
        _apiKey = apiKey;
    }

    public CoinGeckoClient(JsonSerializerOptions serializerOptions) : this(new HttpClientHandler(), serializerOptions)
    {
    }

    public CoinGeckoClient(JsonSerializerOptions serializerOptions, string apiKey) : this(new HttpClientHandler(), serializerOptions, apiKey)
    {
    }


    public CoinGeckoClient(HttpClientHandler httpClientHandler, JsonSerializerOptions serializerOptions) : this(new HttpClient(httpClientHandler, true), serializerOptions)
    {
    }

    public CoinGeckoClient(HttpClientHandler httpClientHandler, JsonSerializerOptions serializerOptions, string apiKey)
      : this(new HttpClient(httpClientHandler, true), serializerOptions, apiKey)
    {
    }


    public CoinGeckoClient(HttpClient httpClient) : this(httpClient, serializerOptions: null)
    {
    }

    public CoinGeckoClient(HttpClient httpClient, string apiKey) : this(httpClient, null, apiKey)
    {
    }


    public CoinGeckoClient(HttpClient httpClient, JsonSerializerOptions serializerOptions)
    {
        _httpClient = httpClient;
        _serializerOptions = serializerOptions ?? new JsonSerializerOptions();
        _serializerOptions.Converters.Add(new LongConverter());
        _serializerOptions.Converters.Add(new DecimalConverter());
        _serializerOptions.Converters.Add(new NullableDecimalConverter());
    }

    public CoinGeckoClient(HttpClient httpClient, JsonSerializerOptions serializerOptions, string apiKey)
    {
        _httpClient = httpClient;
        _serializerOptions = serializerOptions ?? new JsonSerializerOptions();
        _serializerOptions.Converters.Add(new LongConverter());
        _serializerOptions.Converters.Add(new DecimalConverter());
        _serializerOptions.Converters.Add(new NullableDecimalConverter());
        _apiKey = apiKey;
    }

    #endregion Constructors

    #region Properties

    public static CoinGeckoClient Instance => Lazy.Value;

    public ISimpleClient SimpleClient => new SimpleClient(_httpClient, _serializerOptions, _apiKey);
    public IPingClient PingClient => new PingClient(_httpClient, _serializerOptions, _apiKey);
    public ICoinsClient CoinsClient => new CoinsClient(_httpClient, _serializerOptions, _apiKey);
    public IExchangesClient ExchangesClient => new ExchangesClient(_httpClient, _serializerOptions, _apiKey);
    public IExchangeRatesClient ExchangeRatesClient => new ExchangeRatesClient(_httpClient, _serializerOptions, _apiKey);
    public IGlobalClient GlobalClient => new GlobalClient(_httpClient, _serializerOptions, _apiKey);
    public IContractClient ContractClient => new ContractClient(_httpClient, _serializerOptions, _apiKey);
    public IFinancePlatformsClient FinancePlatformsClient => new FinancePlatformsClient(_httpClient, _serializerOptions, _apiKey);
    public IIndexesClient IndexesClient => new IndexesClient(_httpClient, _serializerOptions, _apiKey);
    public IDerivativesClient DerivativesClient => new DerivativesClient(_httpClient, _serializerOptions, _apiKey);
    public ISearchClient SearchClient => new SearchClient(_httpClient, _serializerOptions, _apiKey);

    #endregion Properties

    #region Methods

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }
        if (disposing)
        {
            _httpClient?.Dispose();
        }
        _isDisposed = true;
    }

    #endregion Methods
}