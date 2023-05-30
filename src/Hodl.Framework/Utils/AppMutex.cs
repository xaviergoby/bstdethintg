namespace Hodl.Utils;

public static class AppMutex
{
    public static readonly string Mutex = $"{Environment.MachineName}.{Guid.NewGuid()}";
}
