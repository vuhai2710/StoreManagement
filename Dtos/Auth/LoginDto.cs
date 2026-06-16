using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Dtos.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
    [MinLength(4, ErrorMessage = "Tên đăng nhập phải có ít nhất 4 ký tự")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(4, ErrorMessage = "Mật khẩu phải có ít nhất 4 ký tự")]
    public string Password { get; set; } = string.Empty;
}
