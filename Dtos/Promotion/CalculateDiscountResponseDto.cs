namespace StoreManagement.Dtos.Promotion;

public class CalculateDiscountResponseDto
{
    public bool Applicable { get; set; }
    public decimal Discount { get; set; }
    public string? DiscountType { get; set; }
    public string? RuleName { get; set; }
    public int? RuleId { get; set; }
}
