namespace Hodl.Api.Utils.Configurations;

public class JwtPolicies
{
    public const string MultiFactorEnabled = "MultiFactorEnabled";

    public const string MultiFactorEnabledClaimName = "mfa";

    public const string MultiFactorClaimValue = "mfa";

    public const string ExpirationTimeClaimName = "exp";

    public static AuthorizationPolicy MultiFactorEnabledPolicy()
    {
        return new AuthorizationPolicyBuilder()
            .RequireClaim(MultiFactorEnabledClaimName, MultiFactorClaimValue)
            .Build();
    }
}
