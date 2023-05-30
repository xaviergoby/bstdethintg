namespace Hodl.Models;

public class ProcessLockModel
{
    public string AppMutex { get; set; }

    public DateTime LockTimeStamp { get; set; }

    public int LockTimeout { get; set; } // Timeout in Seconds
}
