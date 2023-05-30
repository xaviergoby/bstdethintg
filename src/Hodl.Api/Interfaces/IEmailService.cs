namespace Hodl.Api.Interfaces;

public interface IEmailService
{
    Task SendResetPasswordEmailAsync(string email, string token, CancellationToken cancellationToken = default);

    Task SendConfirmationEmailAsync(string email, string token, CancellationToken cancellationToken = default);

    Task SendDisable2FaEmailAsync(string email, string token, CancellationToken cancellationToken = default);

    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
