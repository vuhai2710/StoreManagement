using System.ComponentModel.DataAnnotations;
using StoreManagement.Dtos.Base;

namespace StoreManagement.Dtos.Shipment;

public class ShippingAddressDto : BaseDto
{
    public int? IdShippingAddress { get; set; }
    public int? IdCustomer { get; set; }

    [Required(ErrorMessage = "Tên người nhận không được để trống")]
    public string RecipientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [RegularExpression(@"^[0-9+\-\s]{8,20}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ không được để trống")]
    public string Address { get; set; } = string.Empty;

    public bool? IsDefault { get; set; }
    public int? ProvinceId { get; set; }
    public int? DistrictId { get; set; }
    public string? WardCode { get; set; }
}
