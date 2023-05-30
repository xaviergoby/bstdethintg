using System.Text.Json.Serialization;

namespace CoinMarketCapAPI.Models.CryptoCurrency;

public class Platform
{
    /// <summary>
    /// The unique CoinMarketCap ID for the parent platform cryptocurrency.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// The name of the parent platform cryptocurrency.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// The ticker symbol for the parent platform cryptocurrency.
    /// </summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    /// <summary>
    /// The web URL friendly shorthand version of the parent platform cryptocurrency name.
    /// </summary>
    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    /// <summary>
    /// The token address on the parent platform cryptocurrency.
    /// </summary>
    [JsonPropertyName("token_address")]
    public string TokenAddress { get; set; }
}
