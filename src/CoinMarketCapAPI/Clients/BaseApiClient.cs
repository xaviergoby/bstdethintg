using CoinMarketCapAPI.Models;
using CoinMarketCapAPI.Parameters;
using System.Text.Json;

namespace CoinMarketCapAPI.Clients;

public class BaseApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;


    public BaseApiClient(HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions)
    {
        _httpClient = httpClient;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public Task<TApiResponse> GetAsync<TApiResponse>(Uri resourceUri)
    {
        return SendRequestAsync<TApiResponse>(HttpMethod.Get, resourceUri);
    }

    public async Task<TApiResponse> SendRequestAsync<TApiResponse>(HttpMethod httpMethod, Uri resourseUri)
    {
        var request = new HttpRequestMessage(httpMethod, resourseUri);
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Accept-Encoding", "deflate, gzip");
        request.Headers.Add("X-CMC_PRO_API_KEY", ApiParameters.ApiKey);

        var response = await _httpClient
            .SendAsync(request)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        try
        {
            return JsonSerializer.Deserialize<TApiResponse>(responseContent, _jsonSerializerOptions);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            var errorResponse = JsonSerializer.Deserialize<Dictionary<string, Status>>(responseContent, _jsonSerializerOptions);
            var errorMessage =
                $"Error Code : {errorResponse.Values.First().ErrorCode} Error Message : {errorResponse.Values.First().ErrorMessage}";
            throw new HttpRequestException(errorMessage, e);
        }
    }
}