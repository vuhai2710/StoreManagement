using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Dtos.Promotion;

public class ValidatePromotionRequestDto
{
    [Required(ErrorMessage = "Mã giảm giá không được để trống")]
    public string Code { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Tổng tiền đơn hàng phải lớn hơn hoặc bằng 0")]
    public decimal TotalAmount { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Phí vận chuyển phải lớn hơn hoặc bằng 0")]
    public decimal? ShippingFee { get; set; }

    public string? ExpectedScope { get; set; }
}
