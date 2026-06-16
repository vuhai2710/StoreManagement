using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Auth;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthenticationResponseDto>>> Register([FromBody] RegisterDto request, CancellationToken cancellationToken)
    {
        var response = await _authenticationService.RegisterAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<AuthenticationResponseDto>.Success("Đăng ký thành công", response));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthenticationResponseDto>>> Login([FromBody] LoginDto request, CancellationToken cancellationToken)
    {
        var response = await _authenticationService.AuthenticateAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthenticationResponseDto>.Success("Đăng nhập thành công", response));
    }

    [HttpPost("logout")]
    public ActionResult<ApiResponse<object>> Logout()
    {
        return Ok(ApiResponse<object>.Success("Đăng xuất thành công", null));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var message = await _authenticationService.ForgotPasswordAsync(request.Email, cancellationToken);
        return Ok(ApiResponse<object>.Success(message, null));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var message = await _authenticationService.ResetPasswordAsync(request.Token, request.NewPassword, request.ConfirmPassword, cancellationToken);
        return Ok(ApiResponse<object>.Success(message, null));
    }

    [HttpGet("validate-reset-token")]
    public async Task<ActionResult<ApiResponse<object>>> ValidateResetToken([FromQuery] string token, CancellationToken cancellationToken)
    {
        await _authenticationService.ValidateResetTokenAsync(token, cancellationToken);
        return Ok(ApiResponse<object>.Success("Token hợp lệ", null));
    }
}
