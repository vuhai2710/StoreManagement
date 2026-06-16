using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Dtos.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
    [MinLength(4, ErrorMessage = "Tên đăng nhập phải có ít nhất 4 ký tự")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(4, ErrorMessage = "Mật khẩu phải có ít nhất 4 ký tự")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên khách hàng không được để trống")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [RegularExpression(@"^[0-9+\-\s]{8,20}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string PhoneNumber { get; set; } = string.Empty;

    public string? Address { get; set; }
}
