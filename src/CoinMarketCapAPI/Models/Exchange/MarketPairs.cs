using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.Exchange;

public class MarketPairs
{
    [JsonPropertyName("market_pair")]
    public string MarketPair { get; set; }

    [JsonPropertyName("market_pair_base")]
    public MarketPairBaseClass MarketPairBase { get; set; }

    [JsonPropertyName("market_pair_quote")]
    public MarketPairBaseClass MarketPairQuote { get; set; }

    [JsonPropertyName("quote")]
    public MarketPairsQuote Quote { get; set; }
}