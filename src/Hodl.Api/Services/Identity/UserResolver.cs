using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Hodl.Api.Services.Identity;

public class UserResolver : IUserResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<AppUser> _userManager;

    public UserResolver(
        IHttpContextAccessor httpContextAccessor,
        UserManager<AppUser> userManager
        )
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    /// <summary>
    /// Gets the user that is used on the HttpContext, using a JWT token. If no 
    /// user is logged in, null is returned. If a user is found, the user is 
    /// returned, including it's roles.
    /// </summary>
    /// <returns>The AppUser, including roles that is found on the HttpContext, or null if no user is logged in. </returns>
    public async Task<AppUser> GetUser()
    {
        var userId = _httpContextAccessor
            .HttpContext?
            .User?
            .Claims?
            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?
            .Value;

        // When no userId is found on the context, return null.
        if (string.IsNullOrEmpty(userId))
            return null;

        var user = await _userManager.FindByIdAsync(userId);

        if (user != null)
            user.Roles = await _userManager.GetRolesAsync(user);

        return user;
    }

    /// <summary>
    /// Returns the Unix timestamp, as string, for the expiration of the login.
    /// </summary>
    /// <returns></returns>
    public string GetExpirationTime() =>
        _httpContextAccessor
        .HttpContext?
        .User?
        .Claims
        .FirstOrDefault(x => x.Type.Equals(JwtPolicies.ExpirationTimeClaimName))?.Value;

    /// <summary>
    /// Returns the JWT token given in the header of the request.
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetTokenAsync() =>
        await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

    /// <summary>
    /// Returns the claim of the MFA setting in the JWT token.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsMultiFactorVerified() =>
        await Task.Run(() =>
        {
            var str = _httpContextAccessor
                .HttpContext
                .User?
                .Claims?
                .FirstOrDefault(x => x.Type.Equals(JwtPolicies.MultiFactorEnabledClaimName))?
                .Value;

            return str switch
            {
                JwtPolicies.MultiFactorClaimValue => true,
                _ => false
            };
        });
}
