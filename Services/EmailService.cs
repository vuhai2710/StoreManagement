using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using StoreManagement.Options;

namespace StoreManagement.Services;

public class EmailService : IEmailService
{
    private readonly MailOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<MailOptions> options, ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string email, string username, string resetLink, int tokenExpiryMinutes, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.Username))
        {
            _logger.LogWarning("Mail configuration is missing. Password reset link for {Email}: {ResetLink}", email, resetLink);
            return;
        }

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(_options.Username, _options.Password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_options.Username, _options.FromName),
            Subject = "Đặt lại mật khẩu",
            Body = $"""
                Xin chào {username},

                Bạn đã yêu cầu đặt lại mật khẩu. Link có hiệu lực trong {tokenExpiryMinutes} phút:
                {resetLink}
                """,
            IsBodyHtml = false
        };

        message.To.Add(email);

        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
    }
}
