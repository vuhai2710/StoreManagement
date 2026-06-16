namespace StoreManagement.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string username, string resetLink, int tokenExpiryMinutes, CancellationToken cancellationToken = default);
}
