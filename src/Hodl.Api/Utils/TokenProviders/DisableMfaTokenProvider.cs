using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace Hodl.Api.Extensions.TokenProviders;

public class DisableMfaTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : AppUser
{
    public DisableMfaTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<DisableMfaTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger)
    {
    }
}


public class DisableMfaTokenProviderOptions : DataProtectionTokenProviderOptions
{
    public DisableMfaTokenProviderOptions()
    {
        Name = "DisableMfaTokenProvider";
        TokenLifespan = TimeSpan.FromMinutes(15);
    }
}
