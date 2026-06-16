using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StoreManagement.Dtos.Auth;
using StoreManagement.Exceptions;
using StoreManagement.Models;
using StoreManagement.Options;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IEmailService _emailService;
    private readonly JwtOptions _jwtOptions;
    private readonly PasswordResetOptions _passwordResetOptions;

    public AuthenticationService(
        IUserRepository userRepository,
        ICustomerRepository customerRepository,
        IEmployeeRepository employeeRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IEmailService emailService,
        IOptions<JwtOptions> jwtOptions,
        IOptions<PasswordResetOptions> passwordResetOptions)
    {
        _userRepository = userRepository;
        _customerRepository = customerRepository;
        _employeeRepository = employeeRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailService = emailService;
        _jwtOptions = jwtOptions.Value;
        _passwordResetOptions = passwordResetOptions.Value;
    }

    public async Task<AuthenticationResponseDto> AuthenticateAsync(LoginDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken)
            ?? throw new InvalidOperationException("User không tồn tại");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            throw new InvalidOperationException("Sai mật khẩu");
        }

        var token = await GenerateTokenAsync(user, cancellationToken);
        return new AuthenticationResponseDto
        {
            Token = token,
            Authenticated = true
        };
    }

    public async Task<AuthenticationResponseDto> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.GetByUsernameAsync(request.Username, cancellationToken) is not null)
        {
            throw new ConflictException("Tên đăng nhập đã tồn tại");
        }

        if (await _userRepository.GetByEmailAsync(request.Email, cancellationToken) is not null)
        {
            throw new ConflictException("Email đã được sử dụng");
        }

        if (await _customerRepository.GetByPhoneNumberAsync(request.PhoneNumber, cancellationToken) is not null)
        {
            throw new ConflictException("Số điện thoại đã được sử dụng");
        }

        var user = new Users
        {
            Username = request.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
            Email = request.Email,
            Role = "CUSTOMER",
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var customer = new Customers
        {
            IdUser = user.IdUser,
            CustomerName = request.CustomerName,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber,
            CustomerType = "REGULAR"
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        var token = await GenerateTokenAsync(user, cancellationToken);
        return new AuthenticationResponseDto
        {
            Token = token,
            Authenticated = true
        };
    }

    public async Task<string> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken)
            ?? throw new ResourceNotFoundException($"Không tìm thấy tài khoản với email: {email}");

        if (user.IsActive != true)
        {
            throw new InvalidOperationException("Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
        }

        await _passwordResetTokenRepository.InvalidateAllTokensForUserAsync(user.IdUser, cancellationToken);

        var resetToken = GenerateResetToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_passwordResetOptions.TokenExpiryMinutes);

        await _passwordResetTokenRepository.AddAsync(new PasswordResetTokens
        {
            UserId = user.IdUser,
            Token = resetToken,
            ExpiresAt = expiresAt,
            Used = false,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _passwordResetTokenRepository.SaveChangesAsync(cancellationToken);

        var resetLink = BuildResetLink(user.Role, resetToken);
        await _emailService.SendPasswordResetEmailAsync(email, user.Username, resetLink, _passwordResetOptions.TokenExpiryMinutes, cancellationToken);

        return "Vui lòng kiểm tra email để đặt lại mật khẩu";
    }

    public async Task<string> ResetPasswordAsync(string token, string newPassword, string confirmPassword, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Mật khẩu xác nhận không khớp");
        }

        var resetToken = await _passwordResetTokenRepository.GetByTokenAsync(token, cancellationToken)
            ?? throw InvalidTokenException.Invalid();

        if (resetToken.Used == true)
        {
            throw InvalidTokenException.AlreadyUsed();
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            throw InvalidTokenException.Expired();
        }

        resetToken.User.Password = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);
        resetToken.Used = true;

        await _passwordResetTokenRepository.SaveChangesAsync(cancellationToken);
        return "Đặt lại mật khẩu thành công. Vui lòng đăng nhập với mật khẩu mới.";
    }

    public async Task ValidateResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var resetToken = await _passwordResetTokenRepository.GetByTokenAsync(token, cancellationToken)
            ?? throw InvalidTokenException.Invalid();

        if (resetToken.Used == true)
        {
            throw InvalidTokenException.AlreadyUsed();
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            throw InvalidTokenException.Expired();
        }
    }

    private async Task<string> GenerateTokenAsync(Users user, CancellationToken cancellationToken)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Username),
            new("role", user.Role),
            new("idUser", user.IdUser.ToString())
        };

        if (string.Equals(user.Role, "EMPLOYEE", StringComparison.OrdinalIgnoreCase))
        {
            var employeeId = await _employeeRepository.GetEmployeeIdByUserIdAsync(user.IdUser, cancellationToken);
            if (employeeId.HasValue)
            {
                claims.Add(new Claim("employeeId", employeeId.Value.ToString()));
            }
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SignerKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMilliseconds(_jwtOptions.Expiration);

        var token = new JwtSecurityToken(
            issuer: "store-management.com",
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateResetToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private string BuildResetLink(string role, string token)
    {
        var baseUrl = role switch
        {
            "ADMIN" => _passwordResetOptions.AdminUrl,
            "EMPLOYEE" => _passwordResetOptions.AdminUrl,
            _ => _passwordResetOptions.ClientUrl
        };

        return $"{baseUrl}?token={token}";
    }
}
