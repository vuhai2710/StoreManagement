using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Dtos.Category;

public class CategoryDto
{
    public int? IdCategory { get; set; }

    [Required(ErrorMessage = "Tên danh mục không được để trống")]
    [MaxLength(255, ErrorMessage = "Tên danh mục không được vượt quá 255 ký tự")]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(10, ErrorMessage = "Mã tiền tố không được vượt quá 10 ký tự")]
    public string? CodePrefix { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
