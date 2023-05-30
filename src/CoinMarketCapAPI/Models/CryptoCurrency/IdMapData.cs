using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class IdMapData : CryptoCurrencyDetail
{
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    [JsonPropertyName("first_historical_data")]
    public DateTimeOffset FirstHistoricalData { get; set; }

    [JsonPropertyName("last_historical_data")]
    public DateTimeOffset LastHistoricalData { get; set; }
}