using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.Exchange;

public class QuotesHistoricalData : CryptoCurrencyBase
{
    [JsonPropertyName("quotes")]
    public QuotesHistoricalQuotes[] Quotes { get; set; }
}