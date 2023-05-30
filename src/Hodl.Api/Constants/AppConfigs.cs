namespace Hodl.Api.Constants;

/// <summary>
/// List of constants that are used in AppSettings
/// </summary>
public class AppConfigs
{
    public const int APP_CONFIG_TIMEOUT = 2000;

    public const string DAILY_NAV_TIMESTAMP = "DailyNav.Last";
    public const string ACTIVE_BOOKING_PERIOD = "BookingPeriod.ActivePeriod";
    public const string FUND_LAYER_BOUNDARIES_ALERT_STATE = "Fund.Layers.Boundaries.State";
    public const string FUND_LAYER_BOUNDARIES_STATE = "Fund.Layers.Boundaries.Alerts";
    public const string FUND_CATEGORIES_BOUNDARIES_ALERT_STATE = "Fund.Categories.Boundaries.State";
    public const string FUND_CATEGORIES_BOUNDARIES_STATE = "Fund.Categories.Boundaries.Alerts";

    public const string LISTING_HISTORY_INTERVAL = "Crypto.Listing.HistoryIntervalInSeconds";
    public const string LISTING_HISTORY_SYMBOLS = "Crypto.Listing.HistorySymbols";
    public const string LISTING_HISTORY_SETTING = "Crypto.Listing.HistorySetting";
    public const string LISTING_HISTORY_LASTRUN = "Crypto.Listing.HistoryLastRun";

    public const string NOTIFICATION_URL_BASE_PATH = "Notification.Url.BasePath";

    public const string AVG_PRICE_QUOTE_ASSET_ID = "AvgPrice.QuoteAssetId";

    public const string SANDBOX_CURRENCIES = "Sandbox.Currencies";
    public const string SANDBOX_VALUES = "Sandbox.Values.{user_id}";
}
