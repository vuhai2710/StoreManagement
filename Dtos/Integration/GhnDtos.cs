namespace StoreManagement.Dtos.Integration;

public class GhnBaseResponseDto<T>
{
    public int? Code { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public class GhnProvinceDto
{
    public int ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
}

public class GhnDistrictDto
{
    public int DistrictId { get; set; }
    public string? DistrictName { get; set; }
}

public class GhnWardDto
{
    public string? WardCode { get; set; }
    public string? WardName { get; set; }
}

public class GhnServiceDto
{
    public int? ServiceId { get; set; }
    public int? ServiceTypeId { get; set; }
    public string? ShortName { get; set; }
}

public class GhnCalculateFeeRequestDto
{
    public int? FromDistrictId { get; set; }
    public int? ToDistrictId { get; set; }
    public string? ToWardCode { get; set; }
    public int? ServiceId { get; set; }
    public int? ServiceTypeId { get; set; }
    public int? Weight { get; set; }
    public int? Length { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? InsuranceValue { get; set; }
}

public class GhnCalculateFeeResponseDto
{
    public decimal? Total { get; set; }
    public decimal? ServiceFee { get; set; }
    public decimal? InsuranceFee { get; set; }
    public decimal? PickStationFee { get; set; }
    public decimal? CourierStationFee { get; set; }
    public decimal? CodFee { get; set; }
    public decimal? ReturnFee { get; set; }
    public decimal? R2sFee { get; set; }
}

public class GhnOrderItemDto
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
}

public class GhnCreateOrderRequestDto
{
    public int? FromDistrictId { get; set; }
    public int? ToDistrictId { get; set; }
    public string? ToWardCode { get; set; }
    public string? ToName { get; set; }
    public string? ToPhone { get; set; }
    public string? ToAddress { get; set; }
    public int? Weight { get; set; }
    public int? Length { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? ServiceId { get; set; }
    public int? InsuranceValue { get; set; }
    public int? CodAmount { get; set; }
    public string? Note { get; set; }
    public List<GhnOrderItemDto> Items { get; set; } = [];
    public string? ClientOrderCode { get; set; }
}

public class GhnCreateOrderResponseDto
{
    public string? OrderCode { get; set; }
}

public class GhnOrderInfoDto
{
    public string? OrderCode { get; set; }
    public string? Status { get; set; }
    public string? Note { get; set; }
    public string? ExpectedDeliveryTime { get; set; }
    public int? TotalFee { get; set; }
}

public class GhnExpectedDeliveryTimeRequestDto
{
    public int? FromDistrictId { get; set; }
    public int? ToDistrictId { get; set; }
    public int? ServiceId { get; set; }
    public string? ToWardCode { get; set; }
}

public class GhnTrackingEventDto
{
    public string? Status { get; set; }
    public string? Description { get; set; }
    public DateTime? Time { get; set; }
}

public class GhnTrackingDto
{
    public string? OrderCode { get; set; }
    public string? Status { get; set; }
    public List<GhnTrackingEventDto> Logs { get; set; } = [];
}

public class GhnUpdateOrderRequestDto
{
    public string? OrderCode { get; set; }
    public string? Note { get; set; }
}

public class GhnWebhookDto
{
    public string? OrderCode { get; set; }
    public string? Status { get; set; }
    public string? UpdatedAt { get; set; }
    public string? Note { get; set; }
}