using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Dtos.Auth;

public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "Token không được để trống")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
