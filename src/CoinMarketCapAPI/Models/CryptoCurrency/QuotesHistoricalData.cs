using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class QuotesHistoricalData
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("quotes")]
    public Quotes[] Quotes { get; set; }
}