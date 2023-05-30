using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class MarketPairsLatestData
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("num_market_pairs")]
    public long NumMarketPairs { get; set; }

    [JsonPropertyName("market_pairs")]
    public MarketPairs[] MarketPairs { get; set; }
}