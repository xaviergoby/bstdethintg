using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class QuotesHistoricalQuote
{
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("volume_24h")]
    public decimal Volume24H { get; set; }

    [JsonPropertyName("market_cap")]
    public decimal MarketCap { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset LastUpdated { get; set; }
}