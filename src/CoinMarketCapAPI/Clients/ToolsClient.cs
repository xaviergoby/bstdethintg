using CoinMarketCapAPI.Models;
using CoinMarketCapAPI.Models.Tools;
using System.Text.Json;

namespace CoinMarketCapAPI.Clients;

public class ToolsClient : BaseApiClient, IToolsClient
{
    public ToolsClient(HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions) :
        base(httpClient, jsonSerializerOptions)
    {
    }

    public async Task<ResponseMain<PriceConversionData>> GetPriceConversion(float amount, string id, string symbol,
        string time, string[] convert)
    {
        return await GetAsync<ResponseMain<PriceConversionData>>(ToolsApiUrls.InfoUri(amount, id, symbol, time,
            convert));
    }
}