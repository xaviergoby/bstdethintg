using CoinMarketCapAPI.JSONConverters;
using CoinMarketCapAPI.Parameters;
using System.Net;
using System.Text.Json;

namespace CoinMarketCapAPI.Clients;

public class CoinMarketCapClient : ICoinMarketCapClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public CoinMarketCapClient(string apiEnvironment, string apiKey) :
        this(new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        }, apiEnvironment, apiKey)
    {
    }

    public CoinMarketCapClient(HttpClientHandler httpClientHandler, string apiEnvironment, string apiKey) :
        this(new HttpClient(httpClientHandler), apiEnvironment, apiKey)
    {
    }

    public CoinMarketCapClient(HttpClient httpClient, string apiEnvironment, string apiKey)
    {
        _httpClient = httpClient;
        ApiParameters.ApiEndPoint =
            new Uri($"https://{(apiEnvironment == ApiEnvironment.Pro ? "pro" : "sandbox")}-api.coinmarketcap.com/",
                UriKind.Absolute);
        ApiParameters.ApiKey = apiKey;

        _jsonSerializerOptions = new JsonSerializerOptions()
        {
            Converters = { new LongConverter(), new BoolConverter() }
        };

    }

    public ICryptoCurrencyClient CryptoCurrencyClient => new CryptoCurrencyClient(_httpClient, _jsonSerializerOptions);
    public IExchangeClient ExchangeClient => new ExchangeClient(_httpClient, _jsonSerializerOptions);
    public IToolsClient ToolsClient => new ToolsClient(_httpClient, _jsonSerializerOptions);
    public IGlobalMetricClient GlobalMetricClient => new GlobalMetricsClient(_httpClient, _jsonSerializerOptions);
}