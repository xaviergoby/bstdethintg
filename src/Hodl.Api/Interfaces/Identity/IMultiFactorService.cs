using Hodl.Api.ViewModels.IdentityModels.MultiFactor;

namespace Hodl.Api.Interfaces.Identity;

public interface IMultiFactorService
{
    Task<MultiFactorEnable> EnableMultiFactorAsync();

    Task<List<string>> SetMultiFactorAsync(string input);

    Task DisableMultiFactorAsync();

    Task DisableMultiFactorAsync(Guid userId);

    Task<List<string>> ResetMultiFactorAsync();

    Task<AppUser> VerifyMultiFactorAsync(string inputCode);

    Task DisableMultiFactorTokenRequestAsync(string email);

    Task DisableMultiFactorTokenRecoveryCodeAsync(string email, string token, string recoveryCode);
}
