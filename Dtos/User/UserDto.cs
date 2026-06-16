using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using StoreManagement.Dtos.Base;

namespace StoreManagement.Dtos.User;

public class UserDto : BaseDto
{
    public int? IdUser { get; set; }

    [MinLength(4, ErrorMessage = "Tên đăng nhập phải có ít nhất 4 ký tự")]
    public string? Username { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? Email { get; set; }

    [JsonIgnore]
    [MinLength(4, ErrorMessage = "Mật khẩu phải có ít nhất 4 ký tự")]
    public string? Password { get; set; }

    public string? Role { get; set; }

    public bool? IsActive { get; set; }

    public string? AvatarUrl { get; set; }
}
