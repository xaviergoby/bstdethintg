namespace Hodl.Api.Interfaces;

public interface IBookingPeriodHelper
{
    string GetPreviousBookingPeriod(string bookingPeriod);

    string GetNextBookingPeriod(string bookingPeriod);

    /// <summary>
    /// Calculates the date from a datetime where the reporting should be done. 
    /// This date does not start at 0:00 but at the hour of reporting.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    DateTime NavDate(DateTimeOffset dt);

    /// <summary>
    /// Calculates the reporting period from a datetime. This date does not 
    /// start at 0:00 but at the hour of reporting.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    string CalcBookingPeriod(DateTimeOffset dt);

    /// <summary>
    /// Get the start datetime for the bookingperiod.
    /// </summary>
    /// <param name="bookingPeriod"></param>
    /// <returns></returns>
    DateTimeOffset GetPeriodStartDateTime(string bookingPeriod);

    /// <summary>
    /// Get the end datetime for the bookingperiod.
    /// </summary>
    /// <param name="bookingPeriod"></param>
    /// <returns></returns>
    DateTimeOffset GetPeriodEndDateTime(string bookingPeriod);

    /// <summary>
    /// Gets the end datetime for the Daily NAV.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    DateTimeOffset DailyNavEndDateTime(DateTime date);

    /// <summary>
    /// Returns true if administration fee must be booked in the given booking 
    /// period.
    /// </summary>
    /// <param name="frequency">Yearly number of times the administration fee will be calculated.</param>
    /// <param name="bookingperiod"></param>
    /// <returns>Returns true if administration fee should be booked, false otherwise.</returns>
    bool BookAdministrationFee(int frequency, string bookingperiod);
}
