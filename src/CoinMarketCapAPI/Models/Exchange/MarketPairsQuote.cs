using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.Exchange;

public class MarketPairsQuote
{
    [JsonPropertyName("exchange_reported")]
    public ExchangeReported ExchangeReported { get; set; }

    public Dictionary<string, MarketPairsCurrencyDetail> CurrencyDetail { get; set; }
}