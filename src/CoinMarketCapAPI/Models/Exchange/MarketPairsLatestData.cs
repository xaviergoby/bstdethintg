using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.Exchange;

public class MarketPairsLatestData : CryptoCurrencyBase
{
    [JsonPropertyName("num_market_pairs")]
    public long NumMarketPairs { get; set; }

    [JsonPropertyName("market_pairs")]
    public MarketPairs[] MarketPairs { get; set; }
}