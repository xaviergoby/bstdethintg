namespace Hodl.Api.Extensions.TokenProviders;

public class DisableMfaTotpTokenProvider<TUser> : TotpSecurityStampBasedTokenProvider<TUser> where TUser : AppUser
{
    public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
    {
        return Task.FromResult(false);
    }

    public override async Task<string> GetUserModifierAsync(string purpose, UserManager<TUser> userManager, TUser user)
    {
        var email = await userManager.GetEmailAsync(user);
        return "DisableMfaTokenProvider:" + purpose + ":" + email;
    }
}
