using Hodl.Api.Extensions.TokenProviders;

namespace Hodl.Api.Extensions;

public static class IdentityBuilderExtensions
{
    public static IdentityBuilder AddTokenProviders(this IdentityBuilder builder)
    {
        var userType = builder.UserType;
        return builder
            .AddDefaultTokenProviders()
            .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider)
            .AddTokenProvider<PasswordResetTokenProvider<AppUser>>(nameof(PasswordResetTokenProvider<AppUser>))
            .AddTokenProvider("DisableMfaTotpTokenProvider", typeof(DisableMfaTotpTokenProvider<>).MakeGenericType(userType))
            .AddTokenProvider("DisableMfaTokenProvider", typeof(DisableMfaTokenProvider<>).MakeGenericType(userType))
            .AddTokenProvider("LoginMfaTokenProvider", typeof(LoginMfaTokenProvider<>).MakeGenericType(userType))
            .AddTokenProvider("ConfirmationEmailTokenProvider", typeof(ConfirmationEmailTokenProvider<>).MakeGenericType(userType));
    }
}
