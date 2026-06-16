using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Dtos.Auth;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
    public string NewPassword { get; set; } = string.Empty;
}
