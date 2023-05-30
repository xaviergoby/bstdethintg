namespace Hodl.Api.ViewModels.Base;

[Route("[controller]")]
[Authorize(Policy = JwtPolicies.MultiFactorEnabled)]
public class MultiFactorProtectedController : BaseController
{
    public MultiFactorProtectedController(IMapper mapper, ILogger<MultiFactorProtectedController> logger, IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
    }
}
