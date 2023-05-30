using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.Exchange;

public class Urls
{
    [JsonPropertyName("website")]
    public Uri[] Website { get; set; }

    [JsonPropertyName("twitter")]
    public Uri[] Twitter { get; set; }

    [JsonPropertyName("blog")]
    public object[] Blog { get; set; }

    [JsonPropertyName("chat")]
    public Uri[] Chat { get; set; }

    [JsonPropertyName("fee")]
    public Uri[] Fee { get; set; }
}