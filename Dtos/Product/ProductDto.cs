using System.ComponentModel.DataAnnotations;
using StoreManagement.Dtos.Base;

namespace StoreManagement.Dtos.Product;

public class ProductDto : BaseDto
{
    public int? IdProduct { get; set; }

    [Required(ErrorMessage = "ID danh mục không được để trống")]
    public int? IdCategory { get; set; }

    public string? CategoryName { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    public string ProductName { get; set; } = string.Empty;

    public string? Brand { get; set; }
    public int? IdSupplier { get; set; }
    public string? SupplierName { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; }
    public string? Status { get; set; }
    public string? ImageUrl { get; set; }
    public List<ProductImageDto>? Images { get; set; }
    public string? ProductCode { get; set; }

    [Required(ErrorMessage = "Loại mã sản phẩm không được để trống")]
    public string CodeType { get; set; } = string.Empty;

    public string? Sku { get; set; }
    public double? AverageRating { get; set; }
    public int? ReviewCount { get; set; }
    public bool? IsDelete { get; set; }
}
