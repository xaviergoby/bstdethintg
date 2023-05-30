using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class MarketPairs
{
    [JsonPropertyName("exchange")]
    public CryptoCurrencyBase Exchange { get; set; }

    [JsonPropertyName("market_pair")]
    public string MarketPair { get; set; }

    [JsonPropertyName("market_pair_base")]
    public MarketPairBaseClass MarketPairBase { get; set; }

    [JsonPropertyName("market_pair_quote")]
    public MarketPairBaseClass MarketPairQuote { get; set; }

    [JsonPropertyName("quote")]
    public Dictionary<string, QuoteConvert> Quote { get; set; }

}