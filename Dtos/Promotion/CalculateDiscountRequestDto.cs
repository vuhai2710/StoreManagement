using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Dtos.Promotion;

public class CalculateDiscountRequestDto
{
    [Required(ErrorMessage = "Tổng tiền đơn hàng không được để trống")]
    [Range(0, double.MaxValue, ErrorMessage = "Tổng tiền đơn hàng phải lớn hơn hoặc bằng 0")]
    public decimal TotalAmount { get; set; }

    public string? CustomerType { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Phí vận chuyển phải lớn hơn hoặc bằng 0")]
    public decimal? ShippingFee { get; set; }
}
