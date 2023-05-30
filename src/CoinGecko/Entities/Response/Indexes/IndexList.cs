using System.Text.Json.Serialization;

namespace CoinGeckoAPI.Entities.Response.Indexes;

public class IndexList
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}