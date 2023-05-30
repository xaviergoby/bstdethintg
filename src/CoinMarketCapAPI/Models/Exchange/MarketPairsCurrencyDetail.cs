using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.Exchange;

public class MarketPairsCurrencyDetail
{
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("volume_24h")]
    public decimal Volume24H { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset LastUpdated { get; set; }
}