using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Hodl.Api.Services.Identity;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IUserResolver _userResolver;
    private readonly IEmailService _emailService;
    private readonly SocialAuthOptions _socialAuthOptions;
    private readonly IChangeLogService _changeLogManager;


    public UserService(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IUserResolver userResolver,
        IEmailService emailService,
        IOptions<SocialAuthOptions> socialOptions,
        IChangeLogService changeLogManager)
    {
        _userResolver = userResolver;
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _socialAuthOptions = socialOptions.Value;
        _changeLogManager = changeLogManager;
    }

    public async Task<AppUser> GetAsync() =>
        await _userResolver.GetUser();

    public async Task<bool> IsMultiFactorVerified() =>
        await _userResolver.IsMultiFactorVerified();

    public async Task<AppUser> UpdateEmailAsync(string email, string password)
    {
        var user = await _userResolver.GetUser();
        var verifiedUser = await LoginAsync(user.Email, password);

        verifiedUser.Email = email ?? verifiedUser.Email;

        var result = await _userManager.UpdateAsync(verifiedUser);

        if (result.Succeeded)
            return verifiedUser;

        throw new RestException(HttpStatusCode.BadRequest, result.Errors.CastAsErrorInformationItems());
    }

    public async Task<AppUser> UpdatePasswordAsync(string currentPassword, string newPassword)
    {
        var user = await _userResolver.GetUser();

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (result.Succeeded)
            return user;

        throw new RestException(HttpStatusCode.BadRequest, result.Errors.CastAsErrorInformationItems());
    }

    #region Auth Region

    public async Task<AppUser> CreateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user != null)
            throw new RestException(HttpStatusCode.UnprocessableEntity,
                new List<ErrorInformationItem>
                {
                    new()
                    {
                        Code = ErrorCodesStore.UserNotAdded,
                        Description = "The e-mail address is already registered."
                    }
                });

        if (!email.EndsWith("@hodl.nl", true, CultureInfo.InvariantCulture))
            throw new RestException(HttpStatusCode.UnprocessableEntity,
                new List<ErrorInformationItem>
                {
                    new()
                    {
                        Code = ErrorCodesStore.UserNotAdded,
                        Description = "Only hodl.nl email adresses are allowed to register."
                    }
                });

        var newUser = new AppUser { Email = email, UserName = email };
        var result = await _userManager.CreateAsync(newUser, password);

        if (!result.Succeeded)
            throw new RestException(HttpStatusCode.BadRequest, result.Errors.CastAsErrorInformationItems());

        var token = await _userManager.GenerateUserTokenAsync(newUser, "ConfirmationEmailTokenProvider", "confirm-email");

        user = await _userManager.FindByEmailAsync(email) ??
            throw new RestException(HttpStatusCode.UnprocessableEntity,
                new ErrorInformationItem
                {
                    Code = ErrorCodesStore.InvalidCredentials,
                    Description = "Email address was not saved."
                });
        try
        {
            await _emailService.SendConfirmationEmailAsync(email, token, cancellationToken);

            // First user in the system is getting Admin role.
            if (_userManager.Users.Count() == 1)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return user;
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

    public async Task<AppUser> LoginAsync(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);

        if (!user.EmailConfirmed)
        {
            throw new RestException(HttpStatusCode.Unauthorized,
                new ErrorInformationItem
                {
                    Code = ErrorCodesStore.RequiresEmailConfirmation,
                    Description = "To login first confirm your email address. A confirmation code is sent to your email address."
                });
        }

        // Do the password check here.
        var result = await _signInManager.CheckPasswordSignInAsync(user, password, true);

        if (result.IsLockedOut)
            throw new RestException(HttpStatusCode.Unauthorized,
                new ErrorInformationItem
                {
                    Code = ErrorCodesStore.AccountLocked,
                    Description = "Account is locked."
                });

        if (result.Succeeded)
        {
            user.Roles = await _userManager.GetRolesAsync(user);

            return user;
        }

        throw new RestException(HttpStatusCode.Unauthorized,
            new ErrorInformationItem
            {
                Code = ErrorCodesStore.InvalidCredentials,
                Description = "E-mail and password combination is incorrect."
            });
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    #endregion

    #region Social Region

    public async Task<AppUser> SignInGoogleAsync(string token)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new List<string>() { _socialAuthOptions.GoogleClientId }
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);

        var user = await _userManager.FindByEmailAsync(payload.Email);

        if (user == null)
        {
            // Register as new user

            if (!payload.Email.EndsWith("@hodl.nl", true, CultureInfo.InvariantCulture))
                throw new RestException(HttpStatusCode.UnprocessableEntity,
                    new List<ErrorInformationItem>
                    {
                        new()
                        {
                            Code = ErrorCodesStore.UserNotAdded,
                            Description = "Only hodl.nl email adresses are allowed to register."
                        }
                    });

            var newUser = new AppUser
            {
                Email = payload.Email,
                UserName = Regex.Replace(payload.Name, @"[^\w\.@-]", ""),
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(newUser);

            if (!result.Succeeded)
                throw new RestException(HttpStatusCode.InternalServerError,
                    new ErrorInformationItem { Code = ErrorCodesStore.UserNotAdded });

            user = await _userManager.FindByEmailAsync(payload.Email);
        }

        if (user != null)
        {
            user.Roles = await _userManager.GetRolesAsync(user);
            return user;
        }

        throw new NotFoundException($"User not found.");
    }

    #endregion

    #region Password Region

    public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await GetUserByEmailAsync(email);
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        try
        {
            await _emailService.SendResetPasswordEmailAsync(email, token, cancellationToken);
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

    public async Task ResetPasswordAsync(string email, string password, string token)
    {
        var user = await GetUserByEmailAsync(email);

        if (await _userManager.IsLockedOutAsync(user))
            throw new RestException(HttpStatusCode.BadRequest,
                new ErrorInformationItem { Code = ErrorCodesStore.AccountLocked });

        var resetPassResult = await _userManager.ResetPasswordAsync(user, token, password);

        if (!resetPassResult.Succeeded)
        {
            var errors = resetPassResult.Errors.CastAsErrorInformationItems().ToList();

            if (errors[0].Code == ErrorCodesStore.InvalidToken)
                await _userManager.AccessFailedAsync(user);

            throw new RestException(HttpStatusCode.BadRequest, errors);
        }
    }

    #endregion

    #region Email Confirmation Region

    public async Task<AppUser> EmailConfirmAsync(string email, string token)
    {
        var appUser = await GetUserByEmailAsync(email);

        var isValid = await _userManager.VerifyUserTokenAsync(appUser,
            "ConfirmationEmailTokenProvider", "confirm-email", token);

        if (!isValid)
            throw new RestException(HttpStatusCode.BadRequest,
                new ErrorInformationItem { Code = ErrorCodesStore.InvalidToken });

        appUser.EmailConfirmed = true;
        var result = await _userManager.UpdateAsync(appUser);

        if (result.Succeeded)
        {
            appUser.Roles = await _userManager.GetRolesAsync(appUser);
            return appUser;
        }

        throw new RestException(HttpStatusCode.BadRequest,
            new ErrorInformationItem { Code = ErrorCodesStore.EmailNotConfirmed });
    }

    public async Task SendEmailConfirmTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        var appUser = await GetUserByEmailAsync(email);

        if (appUser.EmailConfirmed)
            throw new RestException(HttpStatusCode.BadRequest,
                new ErrorInformationItem { Code = "EmailConfirmed" });

        var token = await _userManager.GenerateUserTokenAsync(appUser,
        "ConfirmationEmailTokenProvider", "confirm-email");

        try
        {
            await _emailService.SendConfirmationEmailAsync(email, token, cancellationToken);
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

    #endregion

    #region IdentityManagement

    public async Task<PagingModel<AppUser>> GetUsersAsync(int page, int? itemsPerPage, CancellationToken cancellationToken = default)
    {
        // TODO: Sorting.
        // TODO: Filter by user-role
        // TODO: Check user roles and return results based on role
        var query = _userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.Email);

        // Create a paged resultset
        return await query.PaginateAsync(page, itemsPerPage ?? PagingConstants.DEFAULT_ITEMS_PER_PAGE, cancellationToken);
    }

    public async Task<bool> LockoutUserAsync(Guid userId)
    {
        var user = await GetUserByIdAsync(userId);

        var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        return result.Succeeded;
    }

    public async Task<bool> ResetUserLockoutAsync(Guid userId)
    {
        var user = await GetUserByIdAsync(userId);

        var result = await _userManager.SetLockoutEndDateAsync(user, null);

        return result.Succeeded;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await GetUserByIdAsync(userId);

        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            await _changeLogManager.AddChangeLogAsync("Users", user, null);
        }

        return result.Succeeded;
    }

    private async Task<AppUser> GetUserByIdAsync(Guid userId) =>
        await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId) ??
            throw new NotFoundException($"User with userId {userId} not found.");


    private async Task<AppUser> GetUserByEmailAsync(string email) =>
         await _userManager.FindByEmailAsync(email) ??
            throw new RestException(HttpStatusCode.BadRequest,
                new ErrorInformationItem { Code = ErrorCodesStore.InvalidCredentials });
    #endregion
}
