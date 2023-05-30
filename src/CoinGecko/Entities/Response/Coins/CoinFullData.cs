using CoinGeckoAPI.Entities.Response.Shared;
using System.Text.Json.Serialization;

namespace CoinGeckoAPI.Entities.Response.Coins;

public class CoinFullData : CoinFullDataWithOutMarketData
{
    [JsonPropertyName("market_data")]
    public MarketData MarketData { get; set; }
}

public class CoinFullDataWithOutMarketData : CoinList
{
    [JsonPropertyName("image")]
    public Image Image { get; set; }

    [JsonPropertyName("community_data")]
    public CommunityData CommunityData { get; set; }

    [JsonPropertyName("developer_data")]
    public DeveloperData DeveloperData { get; set; }

    [JsonPropertyName("public_interest_stats")]
    public PublicInterestStats PublicInterestStats { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset? LastUpdated { get; set; }

    [JsonPropertyName("localization")]
    public Dictionary<string, string> Localization { get; set; }
}

public class DeveloperData
{
    [JsonPropertyName("forks")]
    public long Forks { get; set; }

    [JsonPropertyName("stars")]
    public long Stars { get; set; }

    [JsonPropertyName("subscribers")]
    public long Subscribers { get; set; }

    [JsonPropertyName("total_issues")]
    public long TotalIssues { get; set; }

    [JsonPropertyName("closed_issues")]
    public long ClosedIssues { get; set; }

    [JsonPropertyName("pull_requests_merged")]
    public long PullRequestsMerged { get; set; }

    [JsonPropertyName("pull_request_contributors")]
    public long PullRequestContributors { get; set; }

    [JsonPropertyName("code_additions_deletions_4_weeks")]
    public Dictionary<string, long> CodeAdditionsDeletions4Weeks { get; set; }

    [JsonPropertyName("commit_count_4_weeks")]
    public long CommitCount4Weeks { get; set; }

    [JsonPropertyName("last_4_weeks_commit_activity_series")]
    public long[] Last4WeeksCommitActivitySeries { get; set; }
}

public class MarketData : MarketDataOhlcv
{
    [JsonPropertyName("roi")]
    public Roi Roi { get; set; }

    [JsonPropertyName("current_price")]
    public Dictionary<string, decimal?> CurrentPrice { get; set; }

    [JsonPropertyName("market_cap")]
    public Dictionary<string, decimal?> MarketCap { get; set; }

    [JsonPropertyName("total_volume")]
    public Dictionary<string, decimal?> TotalVolume { get; set; }

    [JsonPropertyName("high_24h")]
    public Dictionary<string, decimal?> High24H { get; set; }

    [JsonPropertyName("low_24h")]
    public Dictionary<string, decimal?> Low24H { get; set; }

    [JsonPropertyName("price_change_percentage_7d")]
    public decimal? PriceChangePercentage7D { get; set; }

    [JsonPropertyName("price_change_percentage_14d")]
    public decimal? PriceChangePercentage14D { get; set; }

    [JsonPropertyName("price_change_percentage_30d")]
    public decimal? PriceChangePercentage30D { get; set; }

    [JsonPropertyName("price_change_percentage_60d")]
    public decimal? PriceChangePercentage60D { get; set; }

    [JsonPropertyName("price_change_percentage_200d")]
    public decimal? PriceChangePercentage200D { get; set; }

    [JsonPropertyName("price_change_percentage_1y")]
    public decimal? PriceChangePercentage1Y { get; set; }

    [JsonPropertyName("price_change_24h_in_currency")]
    public Dictionary<string, decimal> PriceChange24HInCurrency { get; set; }

    [JsonPropertyName("price_change_percentage_1h_in_currency")]
    public Dictionary<string, decimal> PriceChangePercentage1HInCurrency { get; set; }

    [JsonPropertyName("price_change_percentage_24h_in_currency")]
    public Dictionary<string, decimal> PriceChangePercentage24HInCurrency { get; set; }

    [JsonPropertyName("price_change_percentage_7d_in_currency")]
    public Dictionary<string, decimal> PriceChangePercentage7DInCurrency { get; set; }

    [JsonPropertyName("price_change_percentage_14d_in_currency")]
    public Dictionary<string, decimal> PriceChangePercentage14DInCurrency { get; set; }

    [JsonPropertyName("price_change_percentage_30d_in_currency")]
    public Dictionary<string, decimal> PriceChangePercentage30DInCurrency { get; set; }

    [JsonPropertyName("price_change_percentage_60d_in_currency")]
    public Dictionary<string, decimal> PriceChangePercentage60DInCurrency { get; set; }

    [JsonPropertyName("price_change_percentage_200d_in_currency")]
    public Dictionary<string, decimal> PriceChangePercentage200DInCurrency { get; set; }

    [JsonPropertyName("price_change_percentage_1y_in_currency")]
    public Dictionary<string, decimal> PriceChangePercentage1YInCurrency { get; set; }

    [JsonPropertyName("market_cap_change_24h_in_currency")]
    public Dictionary<string, decimal> MarketCapChange24HInCurrency { get; set; }

    [JsonPropertyName("market_cap_change_percentage_24h_in_currency")]
    public Dictionary<string, decimal> MarketCapChangePercentage24HInCurrency { get; set; }
}

public class Roi
{
    [JsonPropertyName("times")]
    public decimal? Times { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("percentage")]
    public decimal? Percentage { get; set; }
}