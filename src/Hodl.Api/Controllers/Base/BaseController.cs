namespace Hodl.Api.ViewModels.Base;

[ApiController]
[RequestsLogger()]
[Route("[controller]")]
[Authorize(AuthenticationSchemes = JwtIssuerOptions.Schemes)]
#if DEBUG
[AllowAnonymous]
#endif
public abstract class BaseController : Controller
{
    protected readonly IMapper _mapper;
    protected readonly ILogger<BaseController> _logger;
    protected readonly IErrorManager _errorManager;

    protected BaseController(IMapper mapper, ILogger<BaseController> logger, IErrorManager errorManager)
    {
        _mapper = mapper;
        _logger = logger;
        _errorManager = errorManager;
    }
}
