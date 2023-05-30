namespace Hodl.Utils;

public static class DateTimeHelper
{
    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime epoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return epoch.AddSeconds(unixTimeStamp).ToUniversalTime();
    }

    public static DateTime JavaTimeStampToDateTime(double javaTimeStamp)
    {
        // Java timestamp is milliseconds past epoch
        DateTime epoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return epoch.AddMilliseconds(javaTimeStamp).ToUniversalTime();
    }
}
