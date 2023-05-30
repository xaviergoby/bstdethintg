using Hodl.Api.ViewModels.IdentityModels;

namespace Hodl.Api.Controllers.Identity;

[ApiController]
[RequestsLogger()]
[Route("auth")]
public class AuthController : Controller
{
    private readonly IUserService _userService;
    private readonly IJwtTokenManager _jwtTokenManager;
    private readonly IMapper _mapper;

    public AuthController(
        IUserService userService,
        IJwtTokenManager jwtTokenManager,
        IMapper mapper)
    {
        _userService = userService;
        _jwtTokenManager = jwtTokenManager;
        _mapper = mapper;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserViewModel>> Create([FromBody] UserRegisterModelView request)
    {
        var appUser = await _userService.CreateAsync(request.Email, request.Password);
        return Ok(_mapper.Map<UserViewModel>(appUser));
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserViewModel>> Login([FromBody] UserLoginViewModel request)
    {
        var appUser = await _userService.LoginAsync(request.Email, request.Password);
        var userToken = _jwtTokenManager.CreateUserToken(appUser, false);

        return Ok(_mapper.Map<UserViewModel>(userToken));
    }

    [HttpGet("refresh")]
    public async Task<ActionResult<UserViewModel>> RefreshJwtToken()
    {
        var appUser = await _userService.GetAsync() ??
            throw new RestException(HttpStatusCode.Unauthorized,
                new ErrorInformationItem
                {
                    Code = ErrorCodesStore.InvalidToken,
                    Description = "The used token is not valid, please login again."
                });
        var isMultiFactorVerified = await _userService.IsMultiFactorVerified();
        var userToken = _jwtTokenManager.CreateUserToken(appUser, isMultiFactorVerified);

        return Ok(_mapper.Map<UserViewModel>(userToken));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _jwtTokenManager.DeactivateCurrentAsync();
        await _userService.LogoutAsync();

        return Ok();
    }

    [HttpGet("password/{email}")]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        await _userService.ForgotPasswordAsync(email);
        return Ok();
    }

    [HttpPost("password/{email}")]
    public async Task<IActionResult> ResetPassword(string email, [FromBody] UserPasswordResetModelView request)
    {
        await _userService.ResetPasswordAsync(email, request.Password, request.Token);
        return Ok();
    }


    [HttpGet("verify-email/{email}")]
    public async Task<IActionResult> SendEmailCode(string email)
    {
        await _userService.SendEmailConfirmTokenAsync(email);
        return Ok();
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<UserViewModel>> ConfirmEmail([FromBody] UserEmailConfirmViewModel request)
    {
        var appUser = await _userService.EmailConfirmAsync(request.Email, request.VerifyCode);

        var isMultiFactorVerified = await _userService.IsMultiFactorVerified();
        var userToken = _jwtTokenManager.CreateUserToken(appUser, isMultiFactorVerified);

        return Ok(_mapper.Map<UserViewModel>(userToken));
    }

    #region Social Auth Region

    [HttpPost("oauth")]
    public async Task<ActionResult<UserViewModel>> SignInOAuth2([FromBody] UserSocialMediaLoginModelView request)
    {
        var appUser = request.Provider switch
        {
            "Google" => await _userService.SignInGoogleAsync(request.Token),
            _ => throw new RestException(HttpStatusCode.UnprocessableEntity, description: "Unsupported provider")
        };
        var userToken = _jwtTokenManager.CreateUserToken(appUser, true);

        return Ok(_mapper.Map<UserViewModel>(userToken));
    }

    #endregion
}
