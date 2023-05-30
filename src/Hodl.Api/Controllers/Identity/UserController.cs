using Hodl.Api.ViewModels.IdentityModels;

namespace Hodl.Api.Controllers.Identity;

[Route("user")]
public class UserController : BaseController
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IJwtTokenManager _jwtTokenManager;
    private readonly IUserResolver _userResolver;

    public UserController(
        IUserService userSerice,
        IUserResolver userResolver,
        IRoleService roleService,
        IJwtTokenManager jwtTokenManager,
        IMapper mapper,
        ILogger<UserController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _userService = userSerice;
        _userResolver = userResolver;
        _roleService = roleService;
        _jwtTokenManager = jwtTokenManager;
    }


    [HttpGet]
    public async Task<ActionResult<UserViewModel>> GetUser()
    {
        var appUser = await _userService.GetAsync() ??
            throw new RestException(HttpStatusCode.Unauthorized);

        var userModelView = _mapper.Map<UserViewModel>(appUser);

        userModelView.Token = await _userResolver.GetTokenAsync();
        userModelView.ExpirationTime = _userResolver.GetExpirationTime();

        return Ok(userModelView);
    }

    [HttpPost]
    [Route("update/email")]
    public async Task<ActionResult<UserViewModel>> UpdateEmail([FromBody] UserUpdateEmailModelView updateUser)
    {
        var appUser = await _userService.UpdateEmailAsync(updateUser.Email, updateUser.Password);

        var isMultiFactorVerified = await _userService.IsMultiFactorVerified();
        var userToken = _jwtTokenManager.CreateUserToken(appUser, isMultiFactorVerified);

        return Ok(_mapper.Map<UserViewModel>(userToken));
    }

    [HttpPost]
    [Route("update/password")]
    public async Task<ActionResult<UserViewModel>> UpdatePassword([FromBody] UserUpdatePasswordModelView updateUser)
    {
        var appUser = await _userService.UpdatePasswordAsync(updateUser.OldPassword, updateUser.NewPassword);

        var isMultiFactorVerified = await _userService.IsMultiFactorVerified();
        var userToken = _jwtTokenManager.CreateUserToken(appUser, isMultiFactorVerified);

        return Ok(_mapper.Map<UserViewModel>(userToken));
    }

    [HttpGet]
    [Route("list")]
    [Authorize(Roles = "Admin,LeadTrader,HeadSales")]
    public async Task<ActionResult<PagingViewModel<UserListViewModel>>> GetUsers(int page, int? itemsPerPage)
    {
        var pagedUsers = await _userService.GetUsersAsync(page, itemsPerPage);
        var usersModelView = pagedUsers.Items
            .Select(async u =>
            {
                var umv = _mapper.Map<UserListViewModel>(u);
                var roles = await _roleService.GetRolesAsync(u);
                umv.Roles = roles.Select(r => r.Name).ToArray();

                return umv;
            })
            .Select(a => a.Result)
            .ToList();

        var pagedResult = new PagingViewModel<UserListViewModel>()
        {
            CurrentPage = pagedUsers.CurrentPage,
            TotalItems = pagedUsers.TotalItems,
            TotalPages = pagedUsers.TotalPages,
            Items = usersModelView
        };

        return Ok(pagedResult);
    }

    [HttpGet]
    [Route("{userId}/lockout")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> LockoutUser(Guid userId)
    {
        if (await _userService.LockoutUserAsync(userId))
        {
            return Ok("User is blocked for login");
        }

        return Problem("Lockout the user failed");
    }

    [HttpGet]
    [Route("{userId}/resetlockout")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetLockoutUser(Guid userId)
    {
        if (await _userService.ResetUserLockoutAsync(userId))
        {
            return Ok("User lockout is reset");
        }

        return Problem("Reset lockout failed");
    }

    [HttpDelete]
    [Route("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        if (await _userService.DeleteUserAsync(userId))
        {
            return Ok("User is removed from the system");
        }

        return Problem("Can not remove the user. There are probably references to the record.");
    }
}
