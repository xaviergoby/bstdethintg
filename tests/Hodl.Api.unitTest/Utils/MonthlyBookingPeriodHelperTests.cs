using Microsoft.Extensions.Options;

namespace Hodl.Api.UnitTest.Utils;

internal class MonthlyBookingPeriodHelperTests
{
    private readonly MonthlyBookingPeriodHelper bookingPeriodHelper;

    public MonthlyBookingPeriodHelperTests()
    {
        var options = Options.Create(new AppDefaults()
        {
            DailyNavHour = 16,
            BookingPeriod = "Monthly",
            CloseBookingPeriodHour = 16,
            ReportingTimeZone = "W. Europe Standard Time"
        });

        bookingPeriodHelper = new(options);
    }

    [Test]
    [TestCase("202201", "202112")]
    [TestCase("202211", "202210")]
    [TestCase("202202", "202201")]
    public void PreviousBookingPeriodTestEmpty(string test, string expected)
    {
        var nextPeriod = bookingPeriodHelper.GetPreviousBookingPeriod(test);

        Assert.AreEqual(expected, nextPeriod);
    }

    [Test]
    [TestCase("202201", "202202")]
    [TestCase("202211", "202212")]
    [TestCase("202212", "202301")]
    public void NextBookingPeriodTestEmpty(string test, string expected)
    {
        var nextPeriod = bookingPeriodHelper.GetNextBookingPeriod(test);

        Assert.AreEqual(expected, nextPeriod);
    }

    [Test]
    [TestCase("2022-01-01 15:59:00+01:00", "2021-12-31")]
    [TestCase("2022-01-01 16:00:00+01:00", "2022-01-01")]
    public void NavDateTest(DateTimeOffset dt, DateTime expected)
    {
        var navDate = bookingPeriodHelper.NavDate(dt);

        Assert.IsNotNull(navDate, "Calculated NAV date is empty");
        Assert.AreEqual(expected, navDate);

    }

    [Test]
    [TestCase("2022-01-01 15:59:00+01:00", "202201")]
    [TestCase("2022-01-31 15:59:00+01:00", "202201")]
    [TestCase("2022-01-31 16:00:00+01:00", "202202")]
    public void CalcBookingPeriodTest(DateTimeOffset dt, string expected)
    {
        var bookingPeriod = bookingPeriodHelper.CalcBookingPeriod(dt);

        Assert.IsFalse(string.IsNullOrEmpty(bookingPeriod), "Calculated booking period date is empty");
        Assert.AreEqual(expected, bookingPeriod);
    }

    [Test]
    [TestCase("202201", "2021-12-31 16:00:00+01:00")]
    [TestCase("202202", "2022-01-31 16:00:00+01:00")]
    [TestCase("202212", "2022-11-30 16:00:00+01:00")]
    public void GetPeriodStartDateTime(string bookingPeriod, DateTimeOffset expected)
    {
        var result = bookingPeriodHelper.GetPeriodStartDateTime(bookingPeriod);

        Assert.AreEqual(expected, result);
    }

    [Test]
    [TestCase("202201", "2022-01-31 16:00:00+01:00")]
    [TestCase("202202", "2022-02-28 16:00:00+01:00")]
    [TestCase("202212", "2022-12-31 16:00:00+01:00")]
    public void GetPeriodEndDateTime(string bookingPeriod, DateTimeOffset expected)
    {
        var result = bookingPeriodHelper.GetPeriodEndDateTime(bookingPeriod);

        Assert.AreEqual(expected, result);
    }

    [Test]
    [TestCase("2022-01-31", "2022-01-31 16:00:00+01:00")]
    [TestCase("2022-12-01", "2022-12-01 16:00:00+01:00")]
    public void DailyNavEndDateTime(DateTime date, DateTimeOffset expected)
    {
        var result = bookingPeriodHelper.DailyNavEndDateTime(date);

        Assert.AreEqual(expected, result);
    }

    [Test]
    [TestCase(1, "202001", false)]
    [TestCase(1, "202002", false)]
    [TestCase(1, "202003", false)]
    [TestCase(1, "202004", false)]
    [TestCase(1, "202005", false)]
    [TestCase(1, "202006", false)]
    [TestCase(1, "202007", false)]
    [TestCase(1, "202008", false)]
    [TestCase(1, "202009", false)]
    [TestCase(1, "202010", false)]
    [TestCase(1, "202011", false)]
    [TestCase(1, "202012", true)]
    [TestCase(2, "202001", false)]
    [TestCase(2, "202002", false)]
    [TestCase(2, "202003", false)]
    [TestCase(2, "202004", false)]
    [TestCase(2, "202005", false)]
    [TestCase(2, "202006", true)]
    [TestCase(2, "202007", false)]
    [TestCase(2, "202008", false)]
    [TestCase(2, "202009", false)]
    [TestCase(2, "202010", false)]
    [TestCase(2, "202011", false)]
    [TestCase(2, "202012", true)]
    [TestCase(3, "202001", false)]
    [TestCase(3, "202002", false)]
    [TestCase(3, "202003", false)]
    [TestCase(3, "202004", true)]
    [TestCase(3, "202005", false)]
    [TestCase(3, "202006", false)]
    [TestCase(3, "202007", false)]
    [TestCase(3, "202008", true)]
    [TestCase(3, "202009", false)]
    [TestCase(3, "202010", false)]
    [TestCase(3, "202011", false)]
    [TestCase(3, "202012", true)]
    [TestCase(4, "202001", false)]
    [TestCase(4, "202002", false)]
    [TestCase(4, "202003", true)]
    [TestCase(4, "202004", false)]
    [TestCase(4, "202005", false)]
    [TestCase(4, "202006", true)]
    [TestCase(4, "202007", false)]
    [TestCase(4, "202008", false)]
    [TestCase(4, "202009", true)]
    [TestCase(4, "202010", false)]
    [TestCase(4, "202011", false)]
    [TestCase(4, "202012", true)]
    public void BookAdministrationFee(int frequency, string bookingperiod, bool expected)
    {
        var result = bookingPeriodHelper.BookAdministrationFee(frequency, bookingperiod);

        Assert.AreEqual(expected, result);
    }

}
