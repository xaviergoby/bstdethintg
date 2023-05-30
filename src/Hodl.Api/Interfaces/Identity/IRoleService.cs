namespace Hodl.Api.Interfaces.Identity;

public interface IRoleService
{
    Task<IEnumerable<AppRole>> GetAllRolesAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<AppRole>> GetRolesAsync(AppUser user, CancellationToken cancellationToken = default);

    Task AssignRole(Guid userId, string role);

    Task RemoveFromRole(Guid userId, string role);
}
