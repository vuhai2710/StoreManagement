using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Dtos.Supplier;

public class SupplierDto
{
    public int? IdSupplier { get; set; }

    [Required(ErrorMessage = "Tên nhà cung cấp không được để trống")]
    [MaxLength(255, ErrorMessage = "Tên nhà cung cấp không được vượt quá 255 ký tự")]
    public string SupplierName { get; set; } = string.Empty;

    public string? Address { get; set; }

    [MaxLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    [RegularExpression(@"^[0-9+\-\s]{0,20}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? Email { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
