using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Dtos.Auth;

public class ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;
}
