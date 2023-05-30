using Hodl.Api.ViewModels.IdentityModels;

namespace Hodl.Api.Controllers.Identity;

[Route("roles")]
public class RoleController : BaseController
{
    private readonly string[] traderRoles = new string[] { UserRoles.Trader, UserRoles.LeadTrader };
    private readonly string[] salesRoles = new string[] { UserRoles.Sales, UserRoles.HeadSales };


    private readonly IUserResolver _userResolver;
    private readonly IRoleService _roleService;

    public RoleController(
        IUserResolver userResolver,
        IRoleService roleService,
        IMapper mapper,
        ILogger<RoleController> logger,
        IErrorManager errorManager)
        : base(mapper, logger, errorManager)
    {
        _userResolver = userResolver;
        _roleService = roleService;
    }

    /// <summary>
    /// Get roles for the current user
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleViewModel>>> Get()
    {
        var user = await _userResolver.GetUser();

        if (user == null)
            return Unauthorized("User not logged in");

        var userRoles = await _roleService.GetRolesAsync(user);
        var rolesModelView = userRoles.Select(r => _mapper.Map<RoleViewModel>(r));

        return Ok(rolesModelView);
    }

    /// <summary>
    /// Get all available roles
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("all")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RoleViewModel>>> GetAll()
    {
        var roles = await _roleService.GetAllRolesAsync();

        var rolesModelView = roles
            .Select(r => _mapper.Map<RoleViewModel>(r));

        return Ok(rolesModelView);
    }

    private async Task<bool> CanUserAssignRole(string role)
    {
        var current_user = await _userResolver.GetUser();

        // CHECK IF CURRENT USER HAS AN ADMIN ROLE. IF YES THEN PERFORM ROLE ASSIGNNING RIGHT AWAY
        // CHECK WETHER THE ROLE TO BE ASSIGNED IS TRADER OR LEADTRADER AND IF THE CURRENT USER HAS A LEADTRADER ROLE.
        // CHECK WETHER THE ROLE TO BE ASSIGNED IS SALES OR HEADSALES AND IF THE CURRENT USER HAS A HEADSALES ROLE.
        return current_user.Roles.Contains(UserRoles.Admin) ||
            (current_user.Roles.Contains(UserRoles.LeadTrader) && traderRoles.Contains(role)) ||
            (current_user.Roles.Contains(UserRoles.HeadSales) && salesRoles.Contains(role));
    }

    /// <summary>
    /// Conditionally assign a role to a user(Id).
    /// - Admin CAN assign ANY role to ANYONE
    /// - LeadTrader CAN assign Trader & LeadTrader role to ANYONE
    /// - HeadSales CAN assign Sales & HeadSales role to ANYONE
    /// </summary>
    /// <returns></returns>
    [HttpPut]
    [Route("{userId}/{role}")]
    public async Task<IActionResult> AssignRole(Guid userId, string role)
    {
        if (await CanUserAssignRole(role))
        {
            await _roleService.AssignRole(userId, role);
            return Ok($"Succesfully assigned {role} role to user ({userId}).");
        }

        // Returning a HTTP 403 status code & allowing ASP.NET Core's authentication logic to handle the response with its forbidden handling logic
        return Forbid($"Forbiden to assign role {role}!");
    }

    /// <summary>
    /// Conditionally remove a role from a user(Id).
    /// - Admin CAN remove ANY role from ANYONE
    /// - LeadTrader CAN remove Trader & LeadTrader role from ANYONE (with obv ofc these roles)
    /// - HeadSales CAN remove Sales & HeadSales role from ANYONE (with obv ofc these roles)
    /// </summary>
    /// <returns></returns>
    [HttpDelete]
    [Route("{userId}/{role}")]
    public async Task<IActionResult> RemoveFromRole(Guid userId, string role)
    {
        if (await CanUserAssignRole(role))
        {
            await _roleService.RemoveFromRole(userId, role);
            return Ok($"Succesfully removed {role} role from user ({userId}).");
        }

        // Returning a HTTP 403 status code & allowing ASP.NET Core's authentication logic to handle the response with its forbidden handling logic
        return Forbid($"Forbiden to remove role {role}.");
    }
}