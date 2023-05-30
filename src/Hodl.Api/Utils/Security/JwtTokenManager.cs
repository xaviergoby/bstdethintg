using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Hodl.Api.Utils.Configurations;

public class JwtTokenManager : IJwtTokenManager
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtIssuerOptions _jwtIssuerOptions;
    //private readonly IDistributedCache _cache;

    // Temporary option to locally cache the removed tokens
    private static readonly IList<string> _removedTokens = new List<string>();

    public JwtTokenManager(
        IHttpContextAccessor httpContextAccessor,
        IOptions<JwtIssuerOptions> jwtIssuerOptions
        //IDistributedCache cache
        )
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtIssuerOptions = jwtIssuerOptions.Value;
        //_cache = cache;
    }

    public UserToken CreateUserToken(AppUser user, bool isMultiFactorVerified) =>
        new()
        {
            User = user,
            Token = CreateToken(user, isMultiFactorVerified)
        };

    public async Task<bool> IsCurrentActiveToken() =>
        await IsActiveAsync(GetCurrentAsync());

    public async Task DeactivateCurrentAsync() =>
        await DeactivateAsync(GetCurrentAsync());

    public async Task<bool> IsActiveAsync(string token) =>
        await Task.Run(() => !_removedTokens.Contains(GetKey(token)));
    //await _cache.GetStringAsync(GetKey(token)) == null;

    public async Task DeactivateAsync(string token) =>
        await Task.Run(() => _removedTokens.Add(GetKey(token)));
    //await _cache.SetStringAsync(GetKey(token),
    //    " ", new DistributedCacheEntryOptions
    //    {
    //        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_jwtOptions.Value.ValidForMinutes)
    //    });

    private string GetCurrentAsync()
    {
        var authorizationHeader = _httpContextAccessor
            .HttpContext.Request.Headers["authorization"];

        return authorizationHeader == StringValues.Empty
            ? string.Empty
            : authorizationHeader.Single().Split(" ").Last();
    }

    private static string GetKey(string token) => $"tokens:{token}:deactivated";

    public Token CreateToken(AppUser user, bool mfaVerified)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, _jwtIssuerOptions.JtiGenerator()),
            new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(_jwtIssuerOptions.IssuedAt).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        if (user.TwoFactorEnabled)
            claims.Add(new Claim(
                JwtPolicies.MultiFactorEnabledClaimName,
                mfaVerified
                ? JwtPolicies.MultiFactorClaimValue
                : string.Empty));

        // Add role claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var jwt = new JwtSecurityToken(
            _jwtIssuerOptions.Issuer,
            _jwtIssuerOptions.Audience,
            claims,
            _jwtIssuerOptions.NotBefore,
            _jwtIssuerOptions.Expiration,
            _jwtIssuerOptions.SigningCredentials);

        return new()
        {
            Value = new JwtSecurityTokenHandler().WriteToken(jwt),
            Expiration = jwt.ValidTo
        };
    }
}
