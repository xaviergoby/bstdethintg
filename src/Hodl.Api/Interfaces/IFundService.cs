namespace Hodl.Api.Interfaces;

public record SumRecord
{
    public int NumberOfItems;
    public decimal BTCValue;
    public decimal USDValue;
    public decimal TotalSharePercentage;
}

public interface IFundService
{
    /// <summary>
    /// Gets the latest open bookingperiod for the given fund, or the default 
    /// bookingperiod based on the current date.
    /// </summary>
    /// <param name="_db"></param>
    /// <param name="fundId"></param>
    /// <returns></returns>
    Task<string> CurrentBookingPeriod(Guid fundId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all the available booking periods for a given fund.
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string[]> GetAllBookingPeriods(Guid fundId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a valid booking period value. First checks the given period. If that has
    /// a value and is formatted correctly then it is returned. Otherwise the current
    /// bookingperiod for the fund is returned.
    /// </summary>
    /// <param name="bookingPeriod"></param>
    /// <param name="_db"></param>
    /// <param name="fundId"></param>
    /// <returns></returns>
    Task<string> GetValidBookingPeriod(string bookingPeriod, Guid fundId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get funds paged.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="itemsPerPage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PagingModel<Fund>> GetFunds(int page, int? itemsPerPage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all funds that are active. A fund is active when the startdate 
    /// is in the past and the close date is not set or in the future or when
    /// there are still holdings with an open bookingperiod (not closed).
    /// </summary>
    /// <returns></returns>
    Task<IList<Fund>> GetActiveFunds(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the active funds, sorted on dependency. This means that if a fund 
    /// has investments in an other fund, that fund will be returned first in 
    /// the list. This is specifically for the ordering of closing the 
    /// bookingperiod. The NAV for the fund investing in an other fund can only 
    /// be made when the NAV for that fund is already available.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IList<Fund>> GetActiveFundsSortedByDependency(CancellationToken cancellationToken = default);

    Task<Fund> GetFund(Guid fundId, CancellationToken cancellationToken = default);

    Task<Fund> GetFundDetailed(Guid fundId, CancellationToken ctcancellationToken = default);

    Task AddFund(Fund fund);

    Task UpdateFund(Fund fund);

    Task DeleteFund(Guid fundId);

    /// <summary>
    /// Gets full fund info for all active funds to display in the dashboard.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<Fund>> GetFundCards(CancellationToken cancellationToken = default);

    Task<PagingModel<FundOwner>> GetFundOwners(int page, int? itemsPerPage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all the holdings from the active/last period. Optional filtered 
    /// used or all holdings. It also adds the latest currency retes/listings 
    /// to the holdings to be able to calculate the distributions.
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="filterUnused"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IList<Holding>> GetCurrentFundHoldings(Guid fundId, bool filterUnused = true, bool setLatestListings = true, CancellationToken cancellationToken = default);

    Task<IList<Holding>> GetFundHoldings(Guid fundId, string fromBookingPeriod, string toBookingPeriod, bool filterUnused = true, CancellationToken cancellationToken = default);

    Task<Holding> AddHolding(Holding holding, CancellationToken cancellationToken = default);

    Task<Nav> GetCurrentNav(Guid fundId, CancellationToken cancellationToken = default);

    Task<Nav> GetNavByDate(Guid fundId, DateTimeOffset date, CancellationToken cancellationToken = default);

    Task<string[]> GetFundGategoryGroups(Guid fundId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Selects an existing holding for the cryptocurrency in the current 
    /// booking period. When no holding exists, a new one will be created.
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="CryptoCurrencyId"></param>
    /// <returns></returns>
    Task<Holding> GetOrCreateFundHolding(Guid fundId, Guid? CryptoCurrencyId, string currencyISOCode = null, string bookingPeriod = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Close the given booking period. Selects all the trades and money 
    /// transfers to calculate the end balances of the period and then 
    /// calculates the NAV for the fund. It saves the booking period on all 
    /// transactions and create a report of the bookingperiod.
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="bookingPeriod"></param>
    /// <param name="forceRecalculation">Override for closed booking periods to recalculate</param>
    /// <returns>The follow up booking period</returns>
    Task<string> CloseBookingPeriod(Guid fundId, string bookingPeriod, bool forceRecalculation = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates all booking periods until the current booking period is 
    /// activated. This method can be called when new fund history is entered 
    /// and the complete history of trades and transfers is added to the 
    /// database. It willstart with the last registered booking period on the
    /// holdings and loop untill the current period is reached.
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> CloseAllBookingPeriods(Guid fundId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverts the last closed booking period. It removes the latest holdings, 
    /// the last NAV and resets the PeriodClosedDateTime on the holdings. It 
    /// also resets the HWM and fund shares and total value.
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> RollbackCloseBookingPeriod(Guid fundId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate the end balance of each of the holdings in the fund
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="bookingPeriod"></param>
    /// <returns></returns>
    Task<IList<Holding>> RecalcEndBalances(Guid fundId, string bookingPeriod, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate the share of each of the holdings in the fund
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="bookingPeriod"></param>
    /// <returns></returns>
    Task<IList<Holding>> RecalcPercentages(Guid fundId, string bookingPeriod, CancellationToken cancellationToken = default);

    Task<Nav> GetLastNAVFor(Guid fundId, NavType navType, CancellationToken cancellationToken = default);

    Task<Nav> CreateDailyNAV(Guid fundId, DateTime date, CancellationToken cancellationToken = default);

    Task<bool> CalcDailyNavsForPeriod(Guid fundId, string bookingPeriod, bool forceRecalculation = false, CancellationToken cancellationToken = default);

    Task<IList<Holding>> CalcHoldingDistribution(ICollection<Holding> holdings, CancellationToken cancellationToken = default);

    Task<IDictionary<byte, SumRecord>> CalcLayerDistribution(ICollection<Holding> holdings, CancellationToken cancellationToken = default);

    Task<IDictionary<Guid, SumRecord>> CalcCategoryDistribution(ICollection<FundCategory> fundCategories, ICollection<Holding> holdings, CancellationToken cancellationToken = default);

    Task<IList<FundCategory>> GetFundCategories(Guid fundId, CancellationToken cancellation = default);

    Task<FundCategory> GetFundCategory(Guid fundId, Guid categoryId, CancellationToken cancellationToken = default);

}
