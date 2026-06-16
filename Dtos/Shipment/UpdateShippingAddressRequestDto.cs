using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Dtos.Shipment;

public class UpdateShippingAddressRequestDto
{
    [Required(ErrorMessage = "Tên người nhận không được để trống")]
    public string RecipientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [RegularExpression(@"^[0-9+\-\s]{8,20}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ không được để trống")]
    public string Address { get; set; } = string.Empty;

    public int? ProvinceId { get; set; }
    public int? DistrictId { get; set; }
    public string? WardCode { get; set; }
}
