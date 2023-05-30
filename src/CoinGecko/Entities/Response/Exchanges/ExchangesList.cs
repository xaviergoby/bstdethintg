using System.Text.Json.Serialization;

namespace CoinGeckoAPI.Entities.Response.Exchanges;

public class ExchangesList
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}