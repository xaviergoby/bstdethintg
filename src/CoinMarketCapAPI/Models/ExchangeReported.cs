using System.Text.Json.Serialization;

namespace CoinMarketCapAPI;

public class ExchangeReported
{
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("volume_24h_base")]
    public decimal Volume24HBase { get; set; }

    [JsonPropertyName("volume_24h_quote")]
    public decimal Volume24HQuote { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset LastUpdated { get; set; }
}