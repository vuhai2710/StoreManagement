using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using StoreManagement.Dtos.Base;

namespace StoreManagement.Dtos.Employee;

public class EmployeeDto : BaseDto
{
    public int? IdEmployee { get; set; }
    public int? IdUser { get; set; }
    public bool? IsActive { get; set; }

    [Required(ErrorMessage = "Tên nhân viên không được để trống")]
    public string EmployeeName { get; set; } = string.Empty;

    public DateOnly? HireDate { get; set; }

    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [RegularExpression(@"^[0-9+\-\s]{8,20}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string PhoneNumber { get; set; } = string.Empty;

    public string? Address { get; set; }
    public decimal? BaseSalary { get; set; }

    [MinLength(4, ErrorMessage = "Tên đăng nhập phải có ít nhất 4 ký tự")]
    public string? Username { get; set; }

    [JsonPropertyName("password")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [MinLength(4, ErrorMessage = "Mật khẩu phải có ít nhất 4 ký tự")]
    public string? Password { get; set; }

    public string? Email { get; set; }
}
