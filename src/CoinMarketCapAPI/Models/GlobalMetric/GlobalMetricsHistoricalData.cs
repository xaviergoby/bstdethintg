using System.Text.Json.Serialization;

namespace CoinMarketCapAPI;

public class GlobalMetricsHistoricalData
{
    [JsonPropertyName("quotes")]
    public GlobalMetricsHistoricalQuotes[] Quotes { get; set; }
}