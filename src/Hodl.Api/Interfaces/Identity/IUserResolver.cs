namespace Hodl.Api.Interfaces.Identity;

public interface IUserResolver
{
    Task<AppUser> GetUser();

    string GetExpirationTime();

    Task<string> GetTokenAsync();

    Task<bool> IsMultiFactorVerified();
}
