using Microsoft.Extensions.Options;

namespace Hodl.Api.Utils;

public class MonthlyBookingPeriodHelper : IBookingPeriodHelper
{
    private readonly int _dailyNavHour;
    private readonly int _closeBookingPeriodHour;
    private readonly TimeZoneInfo _reportingTimeZone;

    public MonthlyBookingPeriodHelper(
        IOptions<AppDefaults> settings
        )
    {
        _dailyNavHour = settings.Value.DailyNavHour;
        _closeBookingPeriodHour = settings.Value.CloseBookingPeriodHour;
        _reportingTimeZone = TimeZoneInfo.FindSystemTimeZoneById(settings.Value.ReportingTimeZone);
    }

    public string GetPreviousBookingPeriod(string bookingPeriod)
    {
        // Get the next period based on the month numbers (1-12)
        var year = int.Parse(bookingPeriod[..4]);
        var month = int.Parse(bookingPeriod[4..]);

        month--;

        if (month < 1)
        {
            year--;
            month = 12;
        }

        return $"{year:0000}{month:00}";
    }

    public string GetNextBookingPeriod(string bookingPeriod)
    {
        // Get the next period based on the month numbers (1-12)
        var year = int.Parse(bookingPeriod[..4]);
        var month = int.Parse(bookingPeriod[4..]);

        month++;

        if (month > 12)
        {
            year++;
            month = 1;
        }

        return $"{year:0000}{month:00}";
    }

    public DateTime NavDate(DateTimeOffset dt)
    {
        DateTime reportingTime = TimeZoneInfo.ConvertTimeFromUtc(dt.UtcDateTime, _reportingTimeZone);

        return reportingTime.Hour < _dailyNavHour
            ? reportingTime.Date.AddDays(-1)
            : reportingTime.Date;
    }

    public string CalcBookingPeriod(DateTimeOffset dt)
    {
        DateTime reportingTime = TimeZoneInfo.ConvertTimeFromUtc(dt.UtcDateTime, _reportingTimeZone);

        DateTime reportingDay = reportingTime.Hour < _closeBookingPeriodHour
            ? reportingTime.Date
            : reportingTime.Date.AddDays(1);

        return reportingDay.ToString("yyyyMM");
    }

    public DateTimeOffset GetPeriodStartDateTime(string bookingPeriod)
    {
        var year = int.Parse(bookingPeriod[..4]);
        var month = int.Parse(bookingPeriod[4..]);
        var offset = _reportingTimeZone.BaseUtcOffset;
        var startDateTime = new DateTimeOffset(year, month, 1, _closeBookingPeriodHour, 0, 0, offset).AddDays(-1);

        return startDateTime;
    }

    public DateTimeOffset GetPeriodEndDateTime(string bookingPeriod)
    {
        var year = int.Parse(bookingPeriod[..4]);
        var month = int.Parse(bookingPeriod[4..]);
        var offset = _reportingTimeZone.BaseUtcOffset;
        var endDateTime = new DateTimeOffset(year, month, 1, _closeBookingPeriodHour, 0, 0, offset).AddMonths(1).AddDays(-1);

        return endDateTime;
    }

    public DateTimeOffset DailyNavEndDateTime(DateTime date)
    {
        var offset = _reportingTimeZone.BaseUtcOffset;
        var endDateTime = new DateTimeOffset(date.Year, date.Month, date.Day, _closeBookingPeriodHour, 0, 0, offset);

        return endDateTime;
    }

    public bool BookAdministrationFee(int frequency, string bookingperiod)
    {
        int month = int.Parse(bookingperiod[4..]);
        return month % (12 / frequency) == 0;
    }
}
