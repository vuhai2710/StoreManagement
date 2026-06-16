using StoreManagement.Dtos.Auth;

namespace StoreManagement.Services;

public interface IAuthenticationService
{
    Task<AuthenticationResponseDto> AuthenticateAsync(LoginDto request, CancellationToken cancellationToken = default);
    Task<AuthenticationResponseDto> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default);
    Task<string> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
    Task<string> ResetPasswordAsync(string token, string newPassword, string confirmPassword, CancellationToken cancellationToken = default);
    Task ValidateResetTokenAsync(string token, CancellationToken cancellationToken = default);
}
