using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Integration;
using StoreManagement.Dtos.Shipment;
using StoreManagement.Exceptions;
using StoreManagement.Models;
using StoreManagement.Repositories;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/ghn")]
public class GhnController : ControllerBase
{
    private readonly IGhnService _ghnService;

    public GhnController(IGhnService ghnService)
    {
        _ghnService = ghnService;
    }

    [HttpGet("provinces")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<List<GhnProvinceDto>>>> GetProvinces(CancellationToken cancellationToken)
    {
        var result = await _ghnService.GetProvincesAsync(cancellationToken);
        return Ok(ApiResponse<List<GhnProvinceDto>>.Success("Lấy danh sách tỉnh/thành phố thành công", result));
    }

    [HttpGet("districts")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<List<GhnDistrictDto>>>> GetDistricts([FromQuery] int provinceId, CancellationToken cancellationToken)
    {
        var result = await _ghnService.GetDistrictsAsync(provinceId, cancellationToken);
        return Ok(ApiResponse<List<GhnDistrictDto>>.Success("Lấy danh sách quận/huyện thành công", result));
    }

    [HttpGet("wards")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<List<GhnWardDto>>>> GetWards([FromQuery] int districtId, CancellationToken cancellationToken)
    {
        var result = await _ghnService.GetWardsAsync(districtId, cancellationToken);
        return Ok(ApiResponse<List<GhnWardDto>>.Success("Lấy danh sách phường/xã thành công", result));
    }

    [HttpPost("calculate-fee")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<GhnCalculateFeeResponseDto>>> CalculateFee([FromBody] GhnCalculateFeeRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _ghnService.CalculateShippingFeeAsync(request, cancellationToken);
        return Ok(ApiResponse<GhnCalculateFeeResponseDto>.Success("Tính phí vận chuyển thành công", result));
    }

    [HttpPost("create-order")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<GhnCreateOrderResponseDto>>> CreateOrder([FromBody] GhnCreateOrderRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _ghnService.CreateOrderAsync(request, cancellationToken);
        return Ok(ApiResponse<GhnCreateOrderResponseDto>.Success("Tạo đơn hàng GHN thành công", result));
    }

    [HttpGet("orders/{ghnOrderCode}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<GhnOrderInfoDto>>> GetOrderInfo(string ghnOrderCode, CancellationToken cancellationToken)
    {
        var result = await _ghnService.GetOrderInfoAsync(ghnOrderCode, cancellationToken);
        return Ok(ApiResponse<GhnOrderInfoDto>.Success("Lấy thông tin đơn hàng GHN thành công", result));
    }

    [HttpDelete("orders/{ghnOrderCode}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<object>>> CancelOrder(string ghnOrderCode, [FromQuery] string? reason, CancellationToken cancellationToken)
    {
        await _ghnService.CancelOrderAsync(ghnOrderCode, reason, cancellationToken);
        return Ok(ApiResponse<object>.Success("Hủy đơn hàng GHN thành công", null));
    }

    [HttpGet("services")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<List<GhnServiceDto>>>> GetShippingServices([FromQuery] int fromDistrictId, [FromQuery] int toDistrictId, CancellationToken cancellationToken)
    {
        var result = await _ghnService.GetShippingServicesAsync(fromDistrictId, toDistrictId, cancellationToken);
        return Ok(ApiResponse<List<GhnServiceDto>>.Success("Lấy danh sách dịch vụ vận chuyển thành công", result));
    }

    [HttpPost("expected-delivery-time")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<string?>>> GetExpectedDeliveryTime([FromBody] GhnExpectedDeliveryTimeRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _ghnService.GetExpectedDeliveryTimeAsync(request, cancellationToken);
        return Ok(ApiResponse<string?>.Success("Lấy thời gian giao hàng dự kiến thành công", result));
    }

    [HttpGet("track/{ghnOrderCode}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<GhnTrackingDto>>> TrackOrder(string ghnOrderCode, CancellationToken cancellationToken)
    {
        var result = await _ghnService.TrackOrderAsync(ghnOrderCode, cancellationToken);
        return Ok(ApiResponse<GhnTrackingDto>.Success("Theo dõi đơn hàng thành công", result));
    }

    [HttpGet("print/{ghnOrderCode}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<IActionResult> PrintOrder(string ghnOrderCode, CancellationToken cancellationToken)
    {
        var bytes = await _ghnService.PrintOrderAsync(ghnOrderCode, cancellationToken);
        return File(bytes, "application/pdf", $"van-don-{ghnOrderCode}.pdf");
    }

    [HttpPut("orders")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateOrder([FromBody] GhnUpdateOrderRequestDto request, CancellationToken cancellationToken)
    {
        await _ghnService.UpdateOrderAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Success("Cập nhật đơn hàng GHN thành công", null));
    }
}

[ApiController]
[Route("api/v1/ghn")]
public class GhnWebhookController : ControllerBase
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ISystemSettingService _systemSettingService;

    public GhnWebhookController(IShipmentRepository shipmentRepository, IOrderRepository orderRepository, ISystemSettingService systemSettingService)
    {
        _shipmentRepository = shipmentRepository;
        _orderRepository = orderRepository;
        _systemSettingService = systemSettingService;
    }

    [HttpPost("webhook")]
    public async Task<ActionResult<Dictionary<string, string>>> Webhook([FromBody] GhnWebhookDto webhookDto, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByGhnOrderCodeAsync(webhookDto.OrderCode ?? string.Empty, cancellationToken);
        if (shipment is null)
        {
            return Ok(new Dictionary<string, string> { ["status"] = "warning", ["message"] = "Shipment not found" });
        }

        shipment.GhnStatus = webhookDto.Status;
        shipment.GhnUpdatedAt = DateTime.UtcNow;
        shipment.GhnNote = webhookDto.Note;
        if (string.Equals(webhookDto.Status, "delivered", StringComparison.OrdinalIgnoreCase))
        {
            shipment.ShippingStatus = "DELIVERED";
            shipment.IdOrderNavigation.Status = "COMPLETED";
            shipment.IdOrderNavigation.DeliveredAt = DateTime.UtcNow;
            shipment.IdOrderNavigation.CompletedAt = DateTime.UtcNow;
            shipment.IdOrderNavigation.ReturnWindowDays = await _systemSettingService.GetReturnWindowDaysAsync(cancellationToken);
        }
        else if (string.Equals(webhookDto.Status, "cancel", StringComparison.OrdinalIgnoreCase) && (shipment.IdOrderNavigation.Status == "PENDING" || shipment.IdOrderNavigation.Status == "CONFIRMED"))
        {
            shipment.IdOrderNavigation.Status = "CANCELED";
        }

        await _shipmentRepository.SaveChangesAsync(cancellationToken);
        return Ok(new Dictionary<string, string> { ["status"] = "success", ["message"] = "Webhook processed" });
    }
}

[ApiController]
[Route("api/v1/payments/payos")]
public class PaymentController : ControllerBase
{
    private readonly IPayOsService _payOsService;
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
    private readonly ISystemSettingService _systemSettingService;

    public PaymentController(IPayOsService payOsService, IOrderRepository orderRepository, IInventoryTransactionRepository inventoryTransactionRepository, ISystemSettingService systemSettingService)
    {
        _payOsService = payOsService;
        _orderRepository = orderRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _systemSettingService = systemSettingService;
    }

    [HttpPost("create/{orderId:int}")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object?>>>> CreatePaymentLink(int orderId, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Order không tồn tại với ID: {orderId}");

        if (!string.Equals(order.PaymentMethod, "PAYOS", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Order payment method phải là PAYOS");
        }

        if ((order.FinalAmount ?? 0m) == 0m)
        {
            order.Status = "COMPLETED";
            order.DeliveredAt = DateTime.UtcNow;
            order.CompletedAt = DateTime.UtcNow;
            order.ReturnWindowDays = await _systemSettingService.GetReturnWindowDaysAsync(cancellationToken);
            await _orderRepository.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<Dictionary<string, object?>>.Success("Đơn hàng 0đ, không cần thanh toán PayOS", new Dictionary<string, object?>
            {
                ["paymentLinkUrl"] = $"http://localhost:3003/payment/success?orderId={orderId}",
                ["paymentLinkId"] = null,
                ["orderId"] = orderId
            }));
        }

        var response = await _payOsService.CreatePaymentLinkAsync(order, cancellationToken);
        order.PaymentLinkId = response.Data?.PaymentLinkId;
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<Dictionary<string, object?>>.Success("Tạo payment link thành công", new Dictionary<string, object?>
        {
            ["paymentLinkUrl"] = response.Data?.CheckoutUrl,
            ["paymentLinkId"] = response.Data?.PaymentLinkId,
            ["orderId"] = orderId,
            ["qrCode"] = response.Data?.QrCode
        }));
    }

    [HttpPost("webhook")]
    public async Task<ActionResult<Dictionary<string, string>>> Webhook([FromBody] JsonElement requestBody, CancellationToken cancellationToken)
    {
        var json = requestBody.GetRawText();
        var webhookDto = JsonSerializer.Deserialize<PayOsWebhookDto>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? new PayOsWebhookDto();

        if (!_payOsService.VerifyWebhookSignature(json, webhookDto.Signature))
        {
            return Ok(new Dictionary<string, string> { ["status"] = "error", ["message"] = "Invalid signature" });
        }

        var paymentLinkId = webhookDto.Data?.PaymentLinkId ?? string.Empty;
        var order = await _orderRepository.GetByPaymentLinkIdAsync(paymentLinkId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Order không tồn tại với paymentLinkId: {paymentLinkId}");

        if (order.Status == "PENDING" && webhookDto.Code == "00")
        {
            order.Status = "COMPLETED";
            order.DeliveredAt = DateTime.UtcNow;
            order.CompletedAt = DateTime.UtcNow;
            order.ReturnWindowDays = await _systemSettingService.GetReturnWindowDaysAsync(cancellationToken);
            foreach (var detail in order.OrderDetails)
            {
                await _inventoryTransactionRepository.AddAsync(new InventoryTransactions
                {
                    IdProduct = detail.IdProduct,
                    Quantity = detail.Quantity,
                    TransactionType = "OUT",
                    ReferenceType = "SALE_ORDER",
                    ReferenceId = order.IdOrder,
                    Notes = $"Thanh toán PayOS thành công - Order #{order.IdOrder}",
                    TransactionDate = DateTime.UtcNow
                }, cancellationToken);
            }
            await _inventoryTransactionRepository.SaveChangesAsync(cancellationToken);
        }
        else if (order.Status == "PENDING")
        {
            order.Status = "CANCELED";
        }

        await _orderRepository.SaveChangesAsync(cancellationToken);
        return Ok(new Dictionary<string, string> { ["status"] = "success", ["message"] = "Webhook processed" });
    }

    [HttpGet("return")]
    public ActionResult<string> ReturnUrl([FromQuery] long? orderCode = null, [FromQuery] int? orderId = null)
    {
        var targetOrderId = orderId ?? (orderCode.HasValue ? (int?)orderCode.Value : null);
        var redirectUrl = "http://localhost:3003/payment/success";
        if (targetOrderId.HasValue)
        {
            redirectUrl += $"?orderId={targetOrderId.Value}";
        }

        Response.Headers.Location = redirectUrl;
        return StatusCode(302, "Redirecting to payment success page...");
    }

    [HttpGet("cancel")]
    public ActionResult<string> CancelUrl([FromQuery] long? orderCode = null, [FromQuery] int? orderId = null)
    {
        var targetOrderId = orderId ?? (orderCode.HasValue ? (int?)orderCode.Value : null);
        var redirectUrl = "http://localhost:3003/payment/cancel";
        if (targetOrderId.HasValue)
        {
            redirectUrl += $"?orderId={targetOrderId.Value}";
        }

        Response.Headers.Location = redirectUrl;
        return StatusCode(302, "Redirecting to payment cancel page...");
    }

    [HttpGet("status/{orderId:int}")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object?>>>> GetPaymentStatus(int orderId, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Order không tồn tại với ID: {orderId}");

        return Ok(ApiResponse<Dictionary<string, object?>>.Success("Lấy trạng thái thanh toán thành công", new Dictionary<string, object?>
        {
            ["orderId"] = order.IdOrder,
            ["status"] = order.Status,
            ["paymentMethod"] = order.PaymentMethod,
            ["paymentLinkId"] = order.PaymentLinkId,
            ["finalAmount"] = order.FinalAmount
        }));
    }
}