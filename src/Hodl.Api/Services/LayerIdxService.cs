using Hodl.MarketAPI;

namespace Hodl.Api.Services;


/// <summary>
/// This fund holdings layer indices service provides functionalities for performing operations involving
/// the layer index of holdings in a fund.
/// </summary>
/// Old file name: HoldingsLayerIndexService.cs
public class SimpleLayerIdxService : ILayerIdxService
{
    private const int FOUNDATION_RANKING_LIMIT = 100;
    private const int FLOORLAYER_RANKING_LIMIT = 250;

    private readonly HodlDbContext _db;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBookingPeriodHelper _bookingPeriodHelper;

    public SimpleLayerIdxService(
        HodlDbContext db,
        IServiceProvider serviceProvider,
        IBookingPeriodHelper bookingPeriodHelper
    )
    {
        _db = db;
        _serviceProvider = serviceProvider;
        _bookingPeriodHelper = bookingPeriodHelper;
    }

    /// <summary>
    /// This method is for obtaining the most recent CMC ranking from the Listings table in the database.
    /// It first checks to see if there is a listing which exists (MOST RECENTLY) prior to its bookingperiod start date.
    /// If it does not exist, it then simply fetches the most recent listin
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="_db"></param>
    /// <returns></returns>
    private int GetClosestDbListingCMCRank(Guid cryptoId, DateTime datetime)
    {
        // GetDbListingCMCRan
        // GetClosestDbListingCMCRank
        // GetLatestDbListingCMCRank
        var cryptoId_listing = _db.Listings
            .Where(l => l.CryptoId == cryptoId && l.TimeStamp <= datetime && l.CmcRank > 0)
            .OrderByDescending(l => l.TimeStamp)
            .FirstOrDefault();

        cryptoId_listing ??= _db.Listings
                .Where(l => l.CryptoId == cryptoId && l.CmcRank > 0)
                .OrderBy(l => l.TimeStamp)
                .FirstOrDefault();

        return cryptoId_listing?.CmcRank ?? -1;
    }

    /// <summary>
    /// This method is for obtaining the CMC ranking of a given crypto currency by using CMC's API.  
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="_db"></param>
    /// <param name="_coinMarketCapSettings"></param>
    /// <returns></returns>
    private async Task<int> GetCurrentAPIListingCMCRank(string symbol, CancellationToken cancellationToken)
    {
        // This is specific for the CMC implementation
        var cmcApi = _serviceProvider.GetServices<ICryptoMarketApi>().Single(a => a.Source.Equals(ListingSource.CoinMarketCap));

        return await cmcApi.GetCurrencyRank(symbol, cancellationToken);
    }

    /// <summary>
    /// This method is used for checking whether the CMC ranking of a specific crypto can be obtained from the Listings table.
    /// If not, it is then obtained via the CMC API.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="_db"></param>
    /// <param name="_coinMarketCapSettings"></param>
    /// <returns></returns>
    private async Task<int> GetCMCRank(CryptoCurrency crypto, DateTime dateTime, CancellationToken cancellationToken)
    {
        // Checking to see if a crpytoId entry exists in the Listings table and returning it if it does
        if (_db.Listings.Any(l => l.CryptoId == crypto.Id))
        {
            return GetClosestDbListingCMCRank(crypto.Id, dateTime);
        }

        // Requesting CMC for a the ranking of the crypto associated with cryptoId
        return await GetCurrentAPIListingCMCRank(crypto.Symbol, cancellationToken);
    }


    /// <summary>
    /// This method is what ACTAULLY implements the logic behind determining what the LayerIndex of a newly created Holding should be.
    /// </summary>
    /// <param name="cryptoId"></param>
    /// <param name="_db"></param>
    /// <param name="_coinMarketCapSettings"></param>
    /// <returns></returns>
    public async Task<byte> IdxAssignmentStrategy(Holding holding, CancellationToken cancellationToken = default)
    {
        byte layerIdx = holding.LayerIndex;

        if (holding.CryptoId != null)
        {
            var cryptoId = _db.CryptoCurrencies.Where(c => c.Id == holding.CryptoId).SingleOrDefault();
            // Detect bot ticker and return 0 as the bot holdings will not be
            // calculated in the layers
            if (cryptoId == null || cryptoId.Symbol.EndsWith("-B", StringComparison.InvariantCultureIgnoreCase))
            {
                return layerIdx;
            }

            var periodStart = _bookingPeriodHelper.GetPeriodStartDateTime(holding.BookingPeriod).UtcDateTime;
            var cmcRank = await GetCMCRank(cryptoId, periodStart, cancellationToken);
            if (1 <= cmcRank && cmcRank <= FOUNDATION_RANKING_LIMIT)
            {
                layerIdx = 1;
            }
            else if (FOUNDATION_RANKING_LIMIT < cmcRank && cmcRank <= FLOORLAYER_RANKING_LIMIT)
            {
                layerIdx = 2;
            }
            else
            {
                layerIdx = 3;
            }
        }

        return layerIdx;
    }
}

