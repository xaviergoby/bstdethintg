namespace Hodl.Api.Interfaces.Identity;

public readonly record struct Token(string Value, DateTime Expiration);
public readonly record struct UserToken(AppUser User, Token Token);

public interface IJwtTokenManager
{
    UserToken CreateUserToken(AppUser user, bool isMultiFactorVerified);

    Token CreateToken(AppUser user, bool mfaVerified);

    Task<bool> IsCurrentActiveToken();

    Task DeactivateCurrentAsync();

    Task<bool> IsActiveAsync(string token);

    Task DeactivateAsync(string token);
}
