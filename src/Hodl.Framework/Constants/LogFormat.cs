namespace Hodl.Constants;

public class LogFormat
{
    private const string FORMAT_MESSAGE = "{0}";
    public const string FORMAT_TIMESTAMP_MESSAGE = "{0} - {1}";

    public const string LOG_TIMESTAMP_MESSAGE = "{timestamp} - {message}";

    public static string FormatMessage(string msg) => string.Format(FORMAT_MESSAGE, msg);
}
