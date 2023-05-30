using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.Tools;

public class PriceConversionQuote
{
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset LastUpdated { get; set; }
}