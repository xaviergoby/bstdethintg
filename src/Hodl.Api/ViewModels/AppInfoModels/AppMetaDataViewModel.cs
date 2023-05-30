namespace Hodl.Api.ViewModels.AppInfoModels;

public class AppInfoViewModel
{
    public string RunningEnvironment { get; set; }
    public string APIVersion { get; set; }
    public string BuildDate { get; set; }
    public string HostName { get; set; }
    public string MachineName { get; set; }
    public int ProcessId { get; set; }
    public string RunMutex { get; set; }
    public string LocalTimeZone { get; set; }
    public DateTime CurrentLocalDateTime { get; set; }
    public DateTime CurrentUTCDateTime { get; set; }
    public DateTime ProcessStartTime { get; set; }
    public string RunTime { get; set; }
}

