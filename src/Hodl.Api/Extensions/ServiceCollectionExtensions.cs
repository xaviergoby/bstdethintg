using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Hodl.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwt(this IServiceCollection services, IConfigurationSection jwtConfig)
    {
        var jwtOptions = jwtConfig.Get<JwtOptions>();
        var issuer = "issuer";
        var audience = "audience";
        var validFor = jwtOptions.ValidForMinutes;
        var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions.SecurityKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var tokenValidationParameters = new TokenValidationParameters
        {
            // The signing key must match!
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingCredentials.Key,
            // Validate the JWT Issuer (iss) claim
            ValidateIssuer = true,
            ValidIssuer = issuer,
            // Validate the JWT Audience (aud) claim
            ValidateAudience = true,
            ValidAudience = audience,
            // Validate the token expiry
            ValidateLifetime = true,
            // If you want to allow a certain amount of clock drift, set that here:
            ClockSkew = TimeSpan.Zero
        };

        services
            .AddTransient<JwtTokenManagerMiddleware>()
            .AddOptions()
            .Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = issuer;
                options.Audience = audience;
                options.SigningCredentials = signingCredentials;
                options.ValidFor = TimeSpan.FromMinutes(validFor);
            })
            .AddAuthorization(options =>
            {
                options.AddPolicy(JwtPolicies.MultiFactorEnabled, JwtPolicies.MultiFactorEnabledPolicy());
            })
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = (context) =>
                    {
                        // Get the token from the query parameters when given
                        if (context.Request.Query.ContainsKey("access_token"))
                        {
                            context.Token = context.Request.Query["access_token"];
                            return Task.CompletedTask;
                        }

                        // Extract the token from the Authorization header
                        var token = context.HttpContext.Request.Headers["Authorization"];
                        if (token.Count > 0)
                        {
                            context.Token = token[0].StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                                ? token[0]["Bearer ".Length..].Trim()
                                : token[0];
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    public static IServiceCollection AddMultiFactorAuthentication(this IServiceCollection services) =>
        services
            .AddTransient<MultiFactorMiddleware>();

    public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration config) =>
        services
            .Configure<AppDefaults>(config.GetSection("AppDefaults"))
            .Configure<EmailOptions>(config.GetSection("EmailOptions"))
            .Configure<SocialAuthOptions>(config.GetSection("SocialAuthOptions"))
            .Configure<DiscordOptions>(config.GetSection("DiscordOptions"));
}
