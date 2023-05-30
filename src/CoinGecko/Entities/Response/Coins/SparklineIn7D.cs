using System.Text.Json.Serialization;

namespace CoinGeckoAPI.Entities.Response.Coins;

public class SparklineIn7D
{
    [JsonPropertyName("price")]
    public decimal[] Price { get; set; }
}