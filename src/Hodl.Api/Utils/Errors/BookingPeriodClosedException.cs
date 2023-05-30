namespace Hodl.Api.Utils.Errors;

/// <summary>
/// This is a special exception to catch errors for manipulations on closed 
/// booking periods. Normaly a closed booking period can not be altered. Not 
/// the holdings nor the trades that have been made because they are all 
/// calculated and reported to the outer world.
/// </summary>
public class BookingPeriodClosedException : RestException
{
    public BookingPeriodClosedException()
    {
    }

    public BookingPeriodClosedException(string message) : base(HttpStatusCode.Conflict, message)
    {
    }

    public BookingPeriodClosedException(string message, Exception innerException) : base(HttpStatusCode.Conflict, message, innerException.Message)
    {
    }
}
