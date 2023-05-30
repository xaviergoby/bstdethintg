using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class Urls
{
    [JsonPropertyName("website")]
    public Uri[] Website { get; set; }

    [JsonPropertyName("technical_doc")]
    public Uri[] TechnicalDoc { get; set; }

    [JsonPropertyName("explorer")]
    public Uri[] Explorer { get; set; }

    [JsonPropertyName("source_code")]
    public Uri[] SourceCode { get; set; }

    [JsonPropertyName("message_board")]
    public Uri[] MessageBoard { get; set; }

    [JsonPropertyName("chat")]
    public object[] Chat { get; set; }

    [JsonPropertyName("announcement")]
    public object[] Announcement { get; set; }

    [JsonPropertyName("reddit")]
    public Uri[] Reddit { get; set; }

    [JsonPropertyName("twitter")]
    public Uri[] Twitter { get; set; }
}