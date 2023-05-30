using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models;

public class ResponseMain<T>
{
    [JsonPropertyName("data")]
    public T Data { get; set; }

    [JsonPropertyName("status")]
    public Status Status { get; set; }
}