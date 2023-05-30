using System.Text.Json.Serialization;

namespace CoinMarketCapAPI;

public class CryptoCurrencyBase
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; }
}