using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using StoreManagement.Dtos.Base;

namespace StoreManagement.Dtos.Customer;

public class CustomerDto : BaseDto
{
    public int? IdCustomer { get; set; }
    public int? IdUser { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? CustomerName { get; set; }

    [RegularExpression(@"^[0-9+\-\s]{0,20}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }
    public string? CustomerType { get; set; }
    public bool? IsActive { get; set; }

    [JsonIgnore]
    [MinLength(4, ErrorMessage = "Mật khẩu phải có ít nhất 4 ký tự")]
    public string? Password { get; set; }
}
