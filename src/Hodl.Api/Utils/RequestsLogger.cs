using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace Hodl.Api.Utils;


/// <summary>
/// A subclass of ActionFilterAttribute for creating custom action filters by over-ridding the inherited methods.
/// </summary>
public class RequestsLogger : ActionFilterAttribute
{
    /// <summary>
    /// For the ASP.NET MVC framework calling this custom action filter right before an action method begins execution.
    /// </summary>
    /// <param name="context"></param>
    private static readonly object lockObject = new();
    private static int totRequests = 0;
    private static int succesfulRequests = 0;
    private static int failedRequests = 0;
    private static double elapsedTime = 0.0;

    public static int TotRequests => totRequests;

    public static int SuccesfulRequests => succesfulRequests;

    public static int FailedRequests => failedRequests;

    public static double ElapsedTime => elapsedTime;

    private readonly Stopwatch stopWatch = new();

    /// <summary>
    /// For PRE execution
    /// </summary>
    /// <param name="context"></param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        stopWatch.Start();
        base.OnActionExecuting(context);
    }
    /// <summary>
    /// OnResultExecuted needs to be overriden in order to check the populated HttpContext.Response
    /// The values HttpContext.Response is populated with in OnActionExecuted (c.f. above) are NOT CORRECT.
    /// </summary>
    /// <param name="context"></param>
    public override void OnResultExecuted(ResultExecutedContext context)
    {
        stopWatch.Stop();
        var resStatusCode = context.HttpContext.Response.StatusCode;

        // Use lock to avoid manipulating static variables from different threads at the same time.
        lock (lockObject)
        {
            totRequests++;
            elapsedTime += stopWatch.Elapsed.TotalMilliseconds;
            if (resStatusCode >= 200 && resStatusCode <= 299)
            {
                succesfulRequests++;
            }
            else
            {
                failedRequests++;
            }
        }

        base.OnResultExecuted(context);
    }
}

