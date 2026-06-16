namespace StoreManagement.Dtos.Promotion;

public class ValidatePromotionResponseDto
{
    public bool Valid { get; set; }
    public string? Message { get; set; }
    public decimal Discount { get; set; }
    public string? DiscountType { get; set; }
    public string? Code { get; set; }
    public string? Scope { get; set; }
}
