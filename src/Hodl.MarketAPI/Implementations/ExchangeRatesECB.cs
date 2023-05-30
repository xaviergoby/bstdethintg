using Hodl.Framework;
using System.Globalization;
using System.Xml;

namespace Hodl.MarketAPI.Implementations;

class ExchangeRatesECB : ExternalApi, IFiatMarketAPI
{
    private const string SOURCE_NAME = "European Central Bank";
    private const string SERVICE_URL = @"https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";
    private const string BASE_CURRENCY = "EUR";

    private static DateTime lastFetch = DateTime.MinValue;
    private static readonly Dictionary<string, decimal> exchangeRates = new();

    public string Source => SOURCE_NAME;

    public DateTime DateTime => lastFetch;

    protected override string ApiStateConfigName => "Api.State.Ecb";

    protected override string ApiMessageTitle => "Market service European Central Bank";

    protected override string ApiMessageContent => "The exchange rate data API of the European Central Bank recieved an error.\r\r\n{0}";

    public ExchangeRatesECB(
        IAppConfigService appConfigService,
        INotificationManager notificationManager,
        ILogger<CoinMarketCapApi> logger
        ) : base(appConfigService, notificationManager, logger)
    {
    }

    /// <summary>
    /// Calculates the exchange rate for the currency against the given base currency.
    /// </summary>
    /// <param name="baseCurrency"></param>
    /// <param name="currency"></param>
    /// <returns></returns>
    public async Task<decimal> ExchangeRate(string baseCurrency, string currency)
    {
        if (await DataAvailable() &&
            exchangeRates.ContainsKey(baseCurrency) &&
            exchangeRates.ContainsKey(currency))
        {
            return exchangeRates[baseCurrency] / exchangeRates[currency];
        }

        return 0;
    }

    private async Task<bool> DataAvailable()
    {
        // The ECB refreshes their rates at 16:00 CET every working day.
        // ref: https://www.ecb.europa.eu/stats/policy_and_exchange_rates/euro_reference_exchange_rates/html/index.en.html

        TimeZoneInfo timeInfo = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        DateTime curDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeInfo);

        DateTime checkDate = curDate.TimeOfDay.Hours < 16
            ? curDate.AddDays(-1)
            : curDate;

        if (lastFetch.Date != checkDate.Date)
        {
            _ = await ApiRequestAsync(async () => await FetchReferenceRates());
        }

        return exchangeRates.Count > 1;
    }

    private static async Task<bool> FetchReferenceRates()
    {
        lock (exchangeRates)
        {
            exchangeRates.Clear();
            exchangeRates.Add(BASE_CURRENCY, 1);
        }

        using XmlReader xmlReader = XmlReader.Create(SERVICE_URL,
            new XmlReaderSettings()
            {
                Async = true
            });
        xmlReader.ReadToFollowing("Cube");
        while (await xmlReader.ReadAsync())
        {
            if (xmlReader.NodeType != XmlNodeType.Element) continue;

            lock (exchangeRates)
            {
                if (xmlReader.GetAttribute("time") != null)
                {
                    // Taken from ISO Standard 8601 for Dates
                    // isoTime = "2002-02-10";
                    lastFetch = DateTime.Parse(xmlReader.GetAttribute("time"));
                }
                else if (xmlReader.GetAttribute("currency") != null && xmlReader.GetAttribute("rate") != null)
                {
                    exchangeRates.Add(xmlReader.GetAttribute("currency"), decimal.Parse(xmlReader.GetAttribute("rate"), CultureInfo.InvariantCulture));
                }
            }
        }

        return true;
    }
}
