using CoinGeckoAPI.Entities.Response.Shared;
using CoinGeckoAPI.JSONConverters;
using System.Text.Json.Serialization;

namespace CoinGeckoAPI.Entities.Response.Coins;

public class CoinFullDataById : CoinFullDataWithOutMarketData
{
    [JsonPropertyName("block_time_in_minutes")]
    public long BlockTimeInMinutes { get; set; }

    [JsonPropertyName("categories")]
    public string[] Categories { get; set; }

    [JsonPropertyName("description")]
    public Dictionary<string, string> Description { get; set; }

    [JsonPropertyName("links")]
    public Links Links { get; set; }

    [JsonPropertyName("country_origin")]
    public string CountryOrigin { get; set; }

    [JsonPropertyName("genesis_date")]
    [JsonConverter(typeof(CoinGeckoDateConverter))]
    public DateTimeOffset? GenesisDate { get; set; }

    [JsonPropertyName("market_cap_rank")]
    public long MarketCapRank { get; set; }

    [JsonPropertyName("coingecko_rank")]
    public long CoinGeckoRank { get; set; }

    [JsonPropertyName("coingecko_score")]
    public decimal? CoinGeckoScore { get; set; }

    [JsonPropertyName("developer_score")]
    public decimal? DeveloperScore { get; set; }

    [JsonPropertyName("community_score")]
    public decimal? CommunityScore { get; set; }

    [JsonPropertyName("liquidity_score")]
    public decimal? LiquidityScore { get; set; }

    [JsonPropertyName("public_interest_score")]
    public decimal? PublicInterestScore { get; set; }

    [JsonPropertyName("market_data")]
    public CoinByIdMarketData MarketData { get; set; }

    [JsonPropertyName("status_updates")]
    public object[] StatusUpdates { get; set; }

    [JsonPropertyName("tickers")]
    public Ticker[] Tickers { get; set; }
}

public class CommunityData
{
    [JsonPropertyName("facebook_likes")]
    public long FacebookLikes { get; set; }

    [JsonPropertyName("twitter_followers")]
    public long TwitterFollowers { get; set; }

    [JsonPropertyName("reddit_average_posts_48h")]
    public long RedditAveragePosts48H { get; set; }

    [JsonPropertyName("reddit_average_comments_48h")]
    public long RedditAverageComments48H { get; set; }

    [JsonPropertyName("reddit_subscribers")]
    public long RedditSubscribers { get; set; }

    [JsonPropertyName("reddit_accounts_active_48h")]
    public long RedditAccountsActive48H { get; set; }

    [JsonPropertyName("telegram_channel_user_count")]
    public long TelegramChannelUserCount { get; set; }
}

public class ReposUrl
{
    [JsonPropertyName("github")]
    public Uri[] Github { get; set; }

    [JsonPropertyName("bitbucket")]
    public object[] Bitbucket { get; set; }
}

public class CoinByIdMarketData : MarketData
{
    [JsonPropertyName("ath")]
    public Dictionary<string, decimal?> Ath { get; set; }

    [JsonPropertyName("ath_change_percentage")]
    public Dictionary<string, decimal> AthChangePercentage { get; set; }

    [JsonPropertyName("total_value_locked")]
    public Dictionary<string, decimal> TotalValueLocked { get; set; }

    [JsonPropertyName("ath_date")]
    public Dictionary<string, DateTimeOffset?> AthDate { get; set; }

    [JsonPropertyName("atl")]
    public Dictionary<string, decimal> Atl { get; set; }

    [JsonPropertyName("atl_change_percentage")]
    public Dictionary<string, decimal> AtlChangePercentage { get; set; }

    [JsonPropertyName("atl_date")]
    public Dictionary<string, DateTimeOffset?> AtlDate { get; set; }

    [JsonPropertyName("sparkline_7d")]
    public SparklineIn7D Sparkline7D { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset? LastUpdated { get; set; }
}