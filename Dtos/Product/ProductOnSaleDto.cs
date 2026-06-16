namespace StoreManagement.Dtos.Product;

public class ProductOnSaleDto
{
    public int ProductId { get; set; }
    public string? Name { get; set; }
    public string? Image { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public DateTime? PromotionEndTime { get; set; }
    public string? DiscountLabel { get; set; }
    public string? PromotionName { get; set; }
    public int? RemainingStock { get; set; }
    public int? DiscountPercentage { get; set; }
}
