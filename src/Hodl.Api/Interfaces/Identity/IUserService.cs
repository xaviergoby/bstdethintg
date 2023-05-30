namespace Hodl.Api.Interfaces.Identity;

public interface IUserService
{
    Task<AppUser> GetAsync();

    Task<bool> IsMultiFactorVerified();

    Task<AppUser> CreateAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<AppUser> UpdateEmailAsync(string email, string password);

    Task<AppUser> UpdatePasswordAsync(string currentPassword, string newPassword);

    Task<AppUser> LoginAsync(string email, string password);

    Task LogoutAsync();

    Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(string email, string password, string token);

    Task<AppUser> EmailConfirmAsync(string email, string token);

    Task SendEmailConfirmTokenAsync(string email, CancellationToken cancellationToken = default);

    Task<AppUser> SignInGoogleAsync(string token);

    #region IdentityManagement

    Task<PagingModel<AppUser>> GetUsersAsync(int page, int? itemsPerPage, CancellationToken cancellationToken = default);

    Task<bool> LockoutUserAsync(Guid userId);

    Task<bool> ResetUserLockoutAsync(Guid userId);

    Task<bool> DeleteUserAsync(Guid userId);

    #endregion

}
