using Hodl.Api.ViewModels.ReportModels;

namespace Hodl.Api.Interfaces;

public interface IReportService
{
    public Task<ReportFundViewModel> GetReportInternal(Guid fundId, string bookingPeriod, CancellationToken cancellationToken = default);

    public ReportTradeSummaryViewModel[] GetTradeSummary(Fund fund, Transfer[] transfers, Trade[] trades);
}

