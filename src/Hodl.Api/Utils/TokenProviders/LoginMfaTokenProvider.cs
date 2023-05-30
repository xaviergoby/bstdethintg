using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace Hodl.Api.Extensions.TokenProviders;

public class LoginMfaTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : AppUser
{
    public LoginMfaTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<DisableMfaTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger)
    {
    }
}

public class LoginMfaTokenProviderOptions : DataProtectionTokenProviderOptions
{
    public LoginMfaTokenProviderOptions()
    {
        Name = "LoginMfaTokenProvider";
        TokenLifespan = TimeSpan.FromMinutes(5);
    }
}
