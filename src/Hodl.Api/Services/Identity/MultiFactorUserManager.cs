using Microsoft.Extensions.Options;
using NETCore.Encrypt;

namespace Hodl.Api.Services.Identity;

public class MultiFactorUserManager : UserManager<AppUser>
{
    private readonly IConfiguration _configuration;

    public MultiFactorUserManager(
        IUserStore<AppUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<AppUser> passwordHasher,
        IEnumerable<IUserValidator<AppUser>> userValidators,
        IEnumerable<IPasswordValidator<AppUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<AppUser>> logger,
        IConfiguration configuration)
        : base(
              store,
              optionsAccessor,
              passwordHasher,
              userValidators,
              passwordValidators,
              keyNormalizer,
              errors,
              services,
              logger)
    {
        _configuration = configuration;
    }

    public override string GenerateNewAuthenticatorKey()
    {
        var originalAuthenticatorKey = base.GenerateNewAuthenticatorKey();

        _ = bool.TryParse(_configuration["TwoFactorAuthentication:EncryptionEnabled"], out bool encryptionEnabled);

        var encryptedKey = encryptionEnabled
            ? EncryptProvider.AESEncrypt(originalAuthenticatorKey,
                _configuration["TwoFactorAuthentication:EncryptionKey"])
            : originalAuthenticatorKey;

        return encryptedKey;
    }

    public override async Task<string> GetAuthenticatorKeyAsync(AppUser user)
    {
        var databaseKey = await base.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrEmpty(databaseKey))
        {
            return null;
        }

        _ = bool.TryParse(_configuration["TwoFactorAuthentication:EncryptionEnabled"], out bool encryptionEnabled);

        return encryptionEnabled
            ? EncryptProvider.AESDecrypt(databaseKey, _configuration["TwoFactorAuthentication:EncryptionKey"])
            : databaseKey;
    }

    protected override string CreateTwoFactorRecoveryCode()
    {
        var originalRecoveryCode = base.CreateTwoFactorRecoveryCode();

        _ = bool.TryParse(_configuration["TwoFactorAuthentication:EncryptionEnabled"], out bool encryptionEnabled);

        return encryptionEnabled
            ? EncryptProvider.AESEncrypt(originalRecoveryCode, _configuration["TwoFactorAuthentication:EncryptionKey"])
            : originalRecoveryCode;
    }

    public override async Task<IEnumerable<string>> GenerateNewTwoFactorRecoveryCodesAsync(AppUser user, int number)
    {
        var tokens = await base.GenerateNewTwoFactorRecoveryCodesAsync(user, number);

        var generatedTokens = tokens as string[] ?? tokens.ToArray();
        if (!generatedTokens.Any())
        {
            return generatedTokens;
        }

        _ = bool.TryParse(_configuration["TwoFactorAuthentication:EncryptionEnabled"], out bool encryptionEnabled);

        return encryptionEnabled
            ? generatedTokens
                .Select(token => EncryptProvider.AESDecrypt(token, _configuration["TwoFactorAuthentication:EncryptionKey"]))
            : generatedTokens;
    }

    public override Task<IdentityResult> RedeemTwoFactorRecoveryCodeAsync(AppUser user, string code)
    {
        _ = bool.TryParse(_configuration["TwoFactorAuthentication:EncryptionEnabled"], out bool encryptionEnabled);

        if (encryptionEnabled && !string.IsNullOrEmpty(code))
        {
            code = EncryptProvider.AESEncrypt(code, _configuration["TwoFactorAuthentication:EncryptionKey"]);
        }

        return base.RedeemTwoFactorRecoveryCodeAsync(user, code);
    }
}
