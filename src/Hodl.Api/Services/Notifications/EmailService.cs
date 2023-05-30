using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace Hodl.Api.Services.Notifications;

/**
 * To send e-mails using Google Workspace accounts, you must activate two 
 * factor authentication. Then an app password can be generated to give access 
 * without the 2-factor authentication. The app password can be generated on 
 * the following page: https://myaccount.google.com/apppasswords
 */
public class EmailService : IEmailService
{
    private readonly EmailOptions _configuration;
    private readonly IStringLocalizer<EmailService> _localizer;

    public EmailService(
        IOptions<EmailOptions> config,
        IStringLocalizer<EmailService> localizer
        )
    {
        _configuration = config.Value;
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
    }

    public async Task SendResetPasswordEmailAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        await SendAsync(email,
            _localizer["ResetPasswordEmailTitle"],
            $"{_localizer["ResetPasswordEmailText"]}: {token}",
            cancellationToken
            );
    }

    public async Task SendConfirmationEmailAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        await SendAsync(email,
            _localizer["ConfirmationEmailTitle"],
            $"{_localizer["ConfirmationEmailText"]}: {token}",
            cancellationToken
            );
    }

    public async Task SendDisable2FaEmailAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        await SendAsync(email,
            _localizer["Disable2FaEmailTitle"],
            $"{_localizer["Disable2FaEmailText"]}: {token}",
            cancellationToken
            );
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_configuration.Host) ||
            string.IsNullOrEmpty(_configuration.Email) ||
            string.IsNullOrEmpty(_configuration.Password))
            return;

        var fromAddress = new MailAddress(_configuration.Email, "Hodl Noreply");
        var toAddress = new MailAddress(to);
        var smtp = new SmtpClient
        {
            Host = _configuration.Host,
            Port = _configuration.Port,
            EnableSsl = _configuration.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            //UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, _configuration.Password),
            Timeout = 20000
        };
        using var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        await smtp.SendMailAsync(message, cancellationToken);
    }
}
