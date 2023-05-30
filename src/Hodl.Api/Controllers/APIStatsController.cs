using Hodl.Api.ViewModels.AppInfoModels;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection;

namespace Hodl.Api.Controllers;

[ApiController]
[RequestsLogger()]
[Route("app")]
public class APIStatsController : Controller
{

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDefaults _settings;

    public APIStatsController(
        IHttpContextAccessor httpContextAccessor,
        IOptions<AppDefaults> settings)
    {
        _httpContextAccessor = httpContextAccessor;
        _settings = settings.Value;
    }

    [HttpGet]
    [Route("info")]
    public ActionResult<AppInfoViewModel> GetAppMetaData()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        // Get the "File version" of Hodl.Api.dll
        var apiVersion = assembly.GetName().Version.ToString();
        // Get the build date of the assembly
        var buildDate = System.IO.File.GetLastWriteTime(assembly.Location).ToShortDateString();
        // Get the host name
        var hostName = _httpContextAccessor.HttpContext.Request.Host.Value;
        var localTimeZone = TimeZoneInfo.Local.DisplayName;
        var currentDateTime = DateTime.Now;
        var processStartTime = Process.GetCurrentProcess().StartTime.ToUniversalTime();
        // Get start app process start time
        var utcNow = DateTime.UtcNow;
        // Compute app process total runtime
        TimeSpan runTime = utcNow.Subtract(processStartTime);

        return Ok(new AppInfoViewModel()
        {
            RunningEnvironment = _settings.RunningEnvironment,
            APIVersion = apiVersion,
            BuildDate = buildDate,
            HostName = hostName,
            MachineName = Environment.MachineName,
            RunMutex = AppMutex.Mutex,
            ProcessId = Environment.ProcessId,
            LocalTimeZone = localTimeZone,
            CurrentLocalDateTime = currentDateTime,
            CurrentUTCDateTime = utcNow,
            ProcessStartTime = processStartTime,
            RunTime = runTime.ToString()
        });
    }
    /// <summary>
    /// This ctrl action method is used for obtaining app statistics, e.g. avg elapsed request time,
    /// tot num of requests made, the num of these that were succesfull & unsuccesfull.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("stats")]
    public ActionResult<AppStatsViewModel> GetAppStats()
    {
        return Ok(new AppStatsViewModel()
        {
            AverageResponseTime = RequestsLogger.TotRequests > 0 ? RequestsLogger.ElapsedTime / RequestsLogger.TotRequests : 0,
            TotalNumRequests = RequestsLogger.TotRequests,
            SuccessfulRequests = RequestsLogger.SuccesfulRequests,
            FailedRequests = RequestsLogger.FailedRequests,
        });
    }
}

