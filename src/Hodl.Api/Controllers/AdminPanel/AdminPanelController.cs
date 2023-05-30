using Hodl.Api.ViewModels.AdminPanelModels;

namespace Hodl.Api.Controllers.AdminPanel;

[ApiController]
[Route("admin")]
public class AdminPanelController : BaseController
{
    private readonly IAppConfigService _appConfigService;

    public AdminPanelController(
        IAppConfigService appConfigService,
        IMapper mapper,
        ILogger<AdminPanelController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _appConfigService = appConfigService;
    }

    /// <summary>
    /// Get an overview of all API states
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("system/state")]
#if !DEBUG
    [Authorize(Roles = "Admin")]
#endif
    public async Task<ActionResult<ApiStateViewModel[]>> GetApiStates(CancellationToken ct)
    {
        var apiStates = await _appConfigService.GetAppConfigsBeginsWith<ExternalApiStateModel>("Api.State", default, ct);

        var states = apiStates
            .Select(r =>
            {
                var state = _mapper.Map<ApiStateViewModel>(r.Value);
                state.ApiName = r.Key;
                return state;
            }).ToArray();

        return Ok(states);
    }
}
