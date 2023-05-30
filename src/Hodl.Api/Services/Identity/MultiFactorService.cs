using Google.Authenticator;
using Hodl.Api.ViewModels.IdentityModels.MultiFactor;
using System.Diagnostics;
using System.Reflection;

namespace Hodl.Api.Services.Identity;

public class MultiFactorService : IMultiFactorService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserResolver _userResolver;
    private readonly IEmailService _emailService;

    public MultiFactorService(
        UserManager<AppUser> userManager,
        IUserResolver userResolver,
        IEmailService emailService)
    {
        _userResolver = userResolver;
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<AppUser> VerifyMultiFactorAsync(string input)
    {
        var user = await _userResolver.GetUser();
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        var authenticator = new TwoFactorAuthenticator();
        var isValid = authenticator.ValidateTwoFactorPIN(key, input);

        if (!isValid)
        {
            throw new RestException(HttpStatusCode.BadRequest, new ErrorInformationItem { Code = ErrorCodesStore.InvalidToken });
        }
        user.Roles = await _userManager.GetRolesAsync(user);

        return user;
    }

    public async Task<MultiFactorEnable> EnableMultiFactorAsync()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        var productName = fvi.ProductName;
        //var productName = Assembly.GetEntryAssembly().GetName().Name;

        var user = await _userResolver.GetUser();

        if (user.TwoFactorEnabled)
            throw new RestException(HttpStatusCode.BadRequest, new ErrorInformationItem
            {
                Code = ErrorCodesStore.UserMultiFactorAlreadyEnabled
            });

        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var authenticator = new TwoFactorAuthenticator();

        var setupCode = authenticator.GenerateSetupCode(
            productName,
            user.Email,
            unformattedKey,
            false, 2);

        var entryKey = setupCode.ManualEntryKey;
        var qrUrl = setupCode.QrCodeSetupImageUrl;
        var key = entryKey.SplitFormat(4, ' ').ToLowerInvariant();

        return new MultiFactorEnable
        {
            Key = key,
            QrImgSrc = qrUrl
        };
    }

    public async Task<List<string>> SetMultiFactorAsync(string input)
    {
        var user = await _userResolver.GetUser();

        if (user.TwoFactorEnabled)
            throw new RestException(HttpStatusCode.BadRequest, new ErrorInformationItem
            {
                Code = ErrorCodesStore.UserMultiFactorAlreadyEnabled
            });

        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        var authenticator = new TwoFactorAuthenticator();
        var isValid = authenticator.ValidateTwoFactorPIN(key, input);

        if (!isValid)
        {
            throw new RestException(HttpStatusCode.BadRequest, new ErrorInformationItem { Code = ErrorCodesStore.InvalidToken });
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        return recoveryCodes.ToList();
    }

    public async Task DisableMultiFactorAsync()
    {
        var user = await _userResolver.GetUser() ??
            throw new RestException(HttpStatusCode.Unauthorized);

        await DisableMultiFactorAsync(user);
    }

    public async Task DisableMultiFactorAsync(Guid userId)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id.Equals(userId)) ??
            throw new RestException(HttpStatusCode.NotFound);

        await DisableMultiFactorAsync(user);
    }

    private async Task DisableMultiFactorAsync(AppUser user)
    {
        if (!await _userManager.GetTwoFactorEnabledAsync(user))
        {
            throw new RestException(HttpStatusCode.BadRequest);
        }

        var result = await _userManager.SetTwoFactorEnabledAsync(user, false);

        if (result.Succeeded)
            return;

        throw new RestException();
    }

    public async Task<List<string>> ResetMultiFactorAsync()
    {
        var user = await _userResolver.GetUser();

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
            throw new RestException(HttpStatusCode.BadRequest);

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        return recoveryCodes.ToList();
    }

    public async Task DisableMultiFactorTokenRequestAsync(string email)
    {
        var AppUser = await _userManager.FindByEmailAsync(email) ??
            throw new RestException(HttpStatusCode.BadRequest,
                new ErrorInformationItem { Code = ErrorCodesStore.InvalidCredentials });

        if (!AppUser.EmailConfirmed)
            throw new RestException(HttpStatusCode.BadRequest,
                new ErrorInformationItem { Code = ErrorCodesStore.EmailNotConfirmed });

        var token = await _userManager.GenerateUserTokenAsync(
            AppUser, "DisableMfaTotpTokenProvider", "disable-two-fa");


        try
        {
            await _emailService.SendDisable2FaEmailAsync(AppUser.Email, token);
        }
        catch (Exception ex)
        {
            throw new RestException(HttpStatusCode.InternalServerError,
                new ErrorInformationItem
                {
                    Code = ErrorCodesStore.EmailNotSended,
                    Description = ex.Message
                });
        }
    }

    public async Task DisableMultiFactorTokenRecoveryCodeAsync(string email, string token, string recoveryCode)
    {
        var AppUser = await _userManager.FindByEmailAsync(email) ??
            throw new RestException(HttpStatusCode.BadGateway,
                new ErrorInformationItem { Code = ErrorCodesStore.InvalidCredentials });


        var isValid = await _userManager.VerifyUserTokenAsync(
            AppUser, "DisableMfaTotpTokenProvider", "disable-two-fa", token);


        if (!isValid)
        {
            throw new RestException(HttpStatusCode.BadRequest,
                new ErrorInformationItem { Code = ErrorCodesStore.InvalidCredentials });
        }

        var result = await _userManager.RedeemTwoFactorRecoveryCodeAsync(AppUser, recoveryCode);

        if (!result.Succeeded)
            throw new RestException(HttpStatusCode.BadRequest,
                result.Errors.CastAsErrorInformationItems());

        var setResult = await _userManager.SetTwoFactorEnabledAsync(AppUser, false);
        if (setResult.Succeeded)
            return;

        throw new RestException(HttpStatusCode.BadGateway,
            setResult.Errors.CastAsErrorInformationItems());
    }
}
