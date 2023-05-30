using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models;

public class Status
{
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("error_code")]
    public long ErrorCode { get; set; }

    [JsonPropertyName("error_message")]
    public string ErrorMessage { get; set; }

    [JsonPropertyName("elapsed")]
    public long Elapsed { get; set; }

    [JsonPropertyName("credit_count")]
    public long CreditCount { get; set; }
}