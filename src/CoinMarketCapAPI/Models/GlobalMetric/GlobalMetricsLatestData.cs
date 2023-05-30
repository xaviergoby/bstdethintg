using System.Text.Json.Serialization;

namespace CoinMarketCapAPI;

public class GlobalMetricsLatestData
{
    [JsonPropertyName("btc_dominance")]
    public decimal BtcDominance { get; set; }

    [JsonPropertyName("eth_dominance")]
    public decimal EthDominance { get; set; }

    [JsonPropertyName("active_cryptocurrencies")]
    public long ActiveCryptocurrencies { get; set; }

    [JsonPropertyName("active_market_pairs")]
    public long ActiveMarketPairs { get; set; }

    [JsonPropertyName("active_exchanges")]
    public long ActiveExchanges { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset LastUpdated { get; set; }

    [JsonPropertyName("quote")]
    public Dictionary<string, GlobalMetricsLatestQuote> Quote { get; set; }
}

public class GlobalMetricsLatestQuote
{
    [JsonPropertyName("total_market_cap")]
    public decimal TotalMarketCap { get; set; }

    [JsonPropertyName("total_volume_24h")]
    public decimal TotalVolume24H { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset LastUpdated { get; set; }
}