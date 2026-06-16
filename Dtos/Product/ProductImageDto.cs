namespace StoreManagement.Dtos.Product;

public class ProductImageDto
{
    public int IdProductImage { get; set; }
    public int IdProduct { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsPrimary { get; set; }
    public int? DisplayOrder { get; set; }
    public DateTime? CreatedAt { get; set; }
}
