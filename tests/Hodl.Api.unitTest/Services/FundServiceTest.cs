using Hodl.Interfaces;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Hodl.Api.UnitTest.Services;
internal class FundServiceTest
{
    private readonly Guid fundId;
    private readonly FundService _fundService;

    public FundServiceTest()
    {
        // The fundId should be initialized first.
        fundId = Guid.NewGuid();

        var holdings = new List<Holding>
        {
            new Holding()
            {
                FundId = fundId,
                BookingPeriod = "202201"
            }
        }.AsQueryable();

        var dbContext = new Mock<HodlDbContext>();
        var dbFacade = new Mock<DatabaseFacade>(dbContext.Object);
        //dbFacade.Setup(x => x.EnsureCreated()).Returns(true);
        //dbFacade.Setup(x => x.BeginTransactionAsync(default)).ReturnsAsync(dbTransaction.Object);
        //dbContext.Setup(db => db.Funds).ReturnsDbSet(funds);
        dbContext.Setup(db => db.Holdings).ReturnsDbSet(holdings);
        //dbContext.Setup(db => db.CryptoCurrencies).ReturnsDbSet(cryptos);
        //dbContext.Setup(db => db.Listings).ReturnsDbSet(new List<Listing>());
        dbContext.Setup(db => db.AppConfigs).ReturnsDbSet(new List<AppConfig>());
        //dbContext.Setup(db => db.Database).Returns(dbFacade.Object);

        var appConfigService = new Mock<IAppConfigService>();
        var changeLogService = new Mock<IChangeLogService>();
        var bookingPeriodHelper = new Mock<IBookingPeriodHelper>();
        bookingPeriodHelper
            .Setup(x => x.CalcBookingPeriod(It.IsAny<DateTimeOffset>()))
            .Returns((DateTimeOffset x) => "202202");

        var logger = new Mock<ILogger<FundService>>();
        var currencyService = new Mock<ICurrencyService>();
        var cryptoCurrencyService = new Mock<ICryptoCurrencyService>();
        var layerIdxService = new Mock<ILayerIdxService>();
        var errorInformationManager = new ErrorManager();

        _fundService = new(
            dbContext.Object,
            bookingPeriodHelper.Object,
            appConfigService.Object,
            currencyService.Object,
            cryptoCurrencyService.Object,
            layerIdxService.Object,
            changeLogService.Object,
            errorInformationManager,
            logger.Object
            );
    }

    [Test]
    public async Task CurrentBookingPeriodTestEmpty()
    {
        var bookingPeriod = await _fundService.CurrentBookingPeriod(Guid.Empty);

        Assert.IsFalse(string.IsNullOrEmpty(bookingPeriod), "Current Bookingperiod is empty");
        Assert.AreEqual("202202", bookingPeriod);
    }

    [Test]
    public async Task CurrentBookingPeriodTestExisting()
    {
        var bookingPeriod = await _fundService.CurrentBookingPeriod(fundId);

        Assert.IsFalse(string.IsNullOrEmpty(bookingPeriod), "Current Bookingperiod is empty");
        Assert.AreEqual("202201", bookingPeriod);
    }

    [Test]
    [TestCase("202106", "202106")]
    [TestCase("202206", "202201")]
    [TestCase("2022", "202201")]
    [TestCase("ABCD01", "202201")]
    public async Task ValidBookingPeriodTest(string test, string expected)
    {
        var bookingPeriod = await _fundService.GetValidBookingPeriod(test, fundId);

        Assert.AreEqual(expected, bookingPeriod);
    }
}
