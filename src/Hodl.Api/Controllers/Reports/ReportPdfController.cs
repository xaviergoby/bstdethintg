using Hodl.Api.Utils.PdfReporting;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using System.Globalization;

namespace Hodl.Api.Controllers.Reports;

[ApiController]
[Route("reports")]
public class ReportPdfController : BaseController
{
    private readonly IReportService _reportService;
    private readonly CultureInfo _cultureInfo;

    public ReportPdfController(
        IReportService reportService,
        IOptions<AppDefaults> settings,
        IMapper mapper,
        ILogger<ReportPdfController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _reportService = reportService;
        _cultureInfo = new CultureInfo(settings.Value.ReportingLocalization);
    }


    /// <summary>
    /// This action method is meant for generating a downloadable pdf report doc for given specific fund & booking period.
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="bookingPeriod"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{fundId}/{bookingPeriod}/pdf")]
    [EnableCors("ContentDisposition")]
#if !DEBUG
    [Authorize(Roles = "Admin,LeadTrader")]
#endif
    public async Task<IActionResult> DownloadFundPdfReport(Guid fundId, string bookingPeriod, CancellationToken ct)
    {
        var reportData = await _reportService.GetReportInternal(fundId, bookingPeriod, ct);
        var document = new PdfReportDocument(reportData, _cultureInfo);
        var generatedPdfDocument = document.GeneratePdf();
        var result = File(generatedPdfDocument, "application/pdf", $"{reportData.FundName.ToLower()}_{bookingPeriod}_report.pdf");

        return result;
    }
}






