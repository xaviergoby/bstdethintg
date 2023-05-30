using System.Text.Json.Serialization;

namespace CoinGeckoAPI.Entities.Response.Coins;

public class AssetPlatform
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("chain_identifier")]
    public long ChainId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("shortname")]
    public string ShortName { get; set; }
}
