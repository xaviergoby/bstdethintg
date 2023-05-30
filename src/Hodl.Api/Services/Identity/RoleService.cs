namespace Hodl.Api.Services.Identity;

public class RoleService : IRoleService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;


    public RoleService(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager
        )
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IEnumerable<AppRole>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _roleManager.Roles
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }


    public async Task<IEnumerable<AppRole>> GetRolesAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new RestException(HttpStatusCode.UnprocessableEntity,
                new ErrorInformationItem
                {
                    Code = ErrorCodesStore.UserNotGiven,
                    Description = "No user given to get user roles for."
                });

        var userRoles = await _userManager.GetRolesAsync(user);
        //if (await _userManager.IsInRoleAsync(user, "Admin"))
        //{
        //    Console.WriteLine("User is admin.");
        //}
        //else
        //{
        //    Console.WriteLine("User is NOT an admin.");
        //}


        return await _roleManager.Roles
            .Where(r => userRoles.Contains(r.Name))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AssignRole(Guid userId, string role)
    {
        var user = await _userManager.Users
            .Where(u => u.Id == userId)
            .SingleOrDefaultAsync() ??
            throw new NotFoundException("User not found");

        await _userManager.AddToRoleAsync(user, role);
    }

    public async Task RemoveFromRole(Guid userId, string role)
    {
        var user = await _userManager.Users
            .Where(u => u.Id == userId)
            .SingleOrDefaultAsync() ??
            throw new NotFoundException("User not found");

        await _userManager.RemoveFromRoleAsync(user, role);
    }
}
