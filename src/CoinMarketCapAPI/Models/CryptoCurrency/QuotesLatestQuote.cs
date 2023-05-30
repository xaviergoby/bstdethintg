using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class QuotesLatestQuote : QuoteVolumePercent
{
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("market_cap")]
    public decimal MarketCap { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset LastUpdated { get; set; }
}