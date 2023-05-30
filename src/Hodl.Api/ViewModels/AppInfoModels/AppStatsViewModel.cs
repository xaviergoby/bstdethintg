namespace Hodl.Api.ViewModels.AppInfoModels;

public class AppStatsViewModel
{
    public int TotalNumRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double AverageResponseTime { get; set; }
}

