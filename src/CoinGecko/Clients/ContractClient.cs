using CoinGeckoAPI.ApiEndPoints;
using CoinGeckoAPI.Entities.Response.Contract;
using CoinGeckoAPI.Interfaces;
using System.Text.Json;

namespace CoinGeckoAPI.Clients;

public class ContractClient : BaseApiClient, IContractClient
{
    public ContractClient(HttpClient httpClient, JsonSerializerOptions serializerOptions) : base(httpClient, serializerOptions)
    {
    }

    public ContractClient(HttpClient httpClient, JsonSerializerOptions serializerOptions, string apiKey) : base(httpClient, serializerOptions, apiKey)
    {
    }

    public async Task<ContractData> GetContractData(string id, string contractAddress)
    {
        return await GetAsync<ContractData>(AppendQueryString(
            ContractApiEndPoints.ContractDetailAddress(id, contractAddress)))
            .ConfigureAwait(false);
    }

    public async Task<MarketChartByContract> GetMarketChartByContract(string id,
        string contractAddress, string vsCurrency, string days)
    {
        return await GetAsync<MarketChartByContract>(AppendQueryString(
            ContractApiEndPoints.MarketChartByContractAddress(id, contractAddress), new Dictionary<string, object>
            {
                {"vs_currency",vsCurrency},
                {"days",days}
            }
        ));
    }

    public async Task<MarketChartRangeByContract> GetMarketChartRangeByContract(string id, string contractAddress, string vsCurrency, string @from, string to)
    {
        return await GetAsync<MarketChartRangeByContract>(AppendQueryString(
            ContractApiEndPoints.MarketChartRangeByContractAddress(id, contractAddress), new Dictionary<string, object>
            {
                {"vs_currency",vsCurrency},
                {"from",from},
                {"to",to},
            }
        ));
    }
}