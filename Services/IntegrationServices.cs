using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using StoreManagement.Dtos.Integration;
using StoreManagement.Dtos.Shipment;
using StoreManagement.Exceptions;
using StoreManagement.Models;
using StoreManagement.Options;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public interface IGhnService
{
    bool IsEnabled();
    Task<List<GhnProvinceDto>> GetProvincesAsync(CancellationToken cancellationToken = default);
    Task<List<GhnDistrictDto>> GetDistrictsAsync(int provinceId, CancellationToken cancellationToken = default);
    Task<List<GhnWardDto>> GetWardsAsync(int districtId, CancellationToken cancellationToken = default);
    Task<GhnCalculateFeeResponseDto> CalculateShippingFeeAsync(GhnCalculateFeeRequestDto request, CancellationToken cancellationToken = default);
    Task<GhnCreateOrderResponseDto> CreateOrderAsync(GhnCreateOrderRequestDto request, CancellationToken cancellationToken = default);
    Task<GhnOrderInfoDto> GetOrderInfoAsync(string ghnOrderCode, CancellationToken cancellationToken = default);
    Task CancelOrderAsync(string ghnOrderCode, string? reason, CancellationToken cancellationToken = default);
    Task<List<GhnServiceDto>> GetShippingServicesAsync(int fromDistrictId, int toDistrictId, CancellationToken cancellationToken = default);
    Task<string?> GetExpectedDeliveryTimeAsync(GhnExpectedDeliveryTimeRequestDto request, CancellationToken cancellationToken = default);
    Task<GhnTrackingDto> TrackOrderAsync(string ghnOrderCode, CancellationToken cancellationToken = default);
    Task<byte[]> PrintOrderAsync(string ghnOrderCode, CancellationToken cancellationToken = default);
    Task UpdateOrderAsync(GhnUpdateOrderRequestDto request, CancellationToken cancellationToken = default);
    Task<string?> CreateGhnOrderAsync(Orders order, ShippingAddresses shippingAddress, CancellationToken cancellationToken = default);
}

public class GhnService : IGhnService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly GhnOptions _options;

    public GhnService(HttpClient httpClient, IOptions<GhnOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public bool IsEnabled() => _options.Enabled;

    public async Task<List<GhnProvinceDto>> GetProvincesAsync(CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        using var document = await SendGetAsync("/shiip/public-api/master-data/province", cancellationToken);
        return ReadList(document, item => new GhnProvinceDto
        {
            ProvinceId = ReadInt(item, "ProvinceID", "ProvinceId", "province_id"),
            ProvinceName = ReadString(item, "ProvinceName", "province_name")
        });
    }

    public async Task<List<GhnDistrictDto>> GetDistrictsAsync(int provinceId, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        using var document = await SendGetAsync($"/shiip/public-api/master-data/district?province_id={provinceId}", cancellationToken);
        return ReadList(document, item => new GhnDistrictDto
        {
            DistrictId = ReadInt(item, "DistrictID", "DistrictId", "district_id"),
            DistrictName = ReadString(item, "DistrictName", "district_name")
        });
    }

    public async Task<List<GhnWardDto>> GetWardsAsync(int districtId, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        using var document = await SendGetAsync($"/shiip/public-api/master-data/ward?district_id={districtId}", cancellationToken);
        return ReadList(document, item => new GhnWardDto
        {
            WardCode = ReadString(item, "WardCode", "ward_code"),
            WardName = ReadString(item, "WardName", "ward_name")
        });
    }

    public async Task<GhnCalculateFeeResponseDto> CalculateShippingFeeAsync(GhnCalculateFeeRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled())
        {
            return DefaultFee();
        }

        try
        {
            if (!request.ServiceId.HasValue && request.FromDistrictId.HasValue && request.ToDistrictId.HasValue)
            {
                var services = await GetShippingServicesAsync(request.FromDistrictId.Value, request.ToDistrictId.Value, cancellationToken);
                var first = services.FirstOrDefault();
                if (first?.ServiceId is not null)
                {
                    request.ServiceId = first.ServiceId;
                    request.ServiceTypeId ??= first.ServiceTypeId;
                }
            }

            if (!request.ServiceId.HasValue && !request.ServiceTypeId.HasValue)
            {
                request.ServiceTypeId = 2;
            }

            var payload = new Dictionary<string, object?>
            {
                ["from_district_id"] = request.FromDistrictId,
                ["to_district_id"] = request.ToDistrictId,
                ["to_ward_code"] = request.ToWardCode,
                ["service_id"] = request.ServiceId,
                ["service_type_id"] = request.ServiceTypeId,
                ["weight"] = request.Weight,
                ["length"] = request.Length,
                ["width"] = request.Width,
                ["height"] = request.Height,
                ["insurance_value"] = request.InsuranceValue
            };

            using var document = await SendPostAsync("/shiip/public-api/v2/shipping-order/fee", payload, cancellationToken);
            var data = GetDataElement(document);
            return new GhnCalculateFeeResponseDto
            {
                Total = ReadDecimal(data, "total"),
                ServiceFee = ReadDecimal(data, "service_fee", "serviceFee"),
                InsuranceFee = ReadDecimal(data, "insurance_fee", "insuranceFee"),
                PickStationFee = ReadDecimal(data, "pick_station_fee", "pickStationFee"),
                CourierStationFee = ReadDecimal(data, "coupon_value", "courier_station_fee", "courierStationFee"),
                CodFee = ReadDecimal(data, "cod_fee", "codFee"),
                ReturnFee = ReadDecimal(data, "return_again", "return_fee", "returnFee"),
                R2sFee = ReadDecimal(data, "r2s_fee", "r2sFee")
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException)
        {
            if (ex.Message.Contains("route not found service", StringComparison.OrdinalIgnoreCase))
            {
                return DefaultFee();
            }

            throw;
        }
    }

    public async Task<GhnCreateOrderResponseDto> CreateOrderAsync(GhnCreateOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        var payload = new Dictionary<string, object?>
        {
            ["from_district_id"] = request.FromDistrictId,
            ["to_district_id"] = request.ToDistrictId,
            ["to_ward_code"] = request.ToWardCode,
            ["to_name"] = request.ToName,
            ["to_phone"] = request.ToPhone,
            ["to_address"] = request.ToAddress,
            ["weight"] = request.Weight,
            ["length"] = request.Length,
            ["width"] = request.Width,
            ["height"] = request.Height,
            ["service_id"] = request.ServiceId,
            ["insurance_value"] = request.InsuranceValue,
            ["cod_amount"] = request.CodAmount,
            ["note"] = request.Note,
            ["items"] = request.Items.Select(item => new Dictionary<string, object?>
            {
                ["name"] = item.Name,
                ["code"] = item.Code,
                ["quantity"] = item.Quantity,
                ["price"] = item.Price
            }).ToList(),
            ["client_order_code"] = request.ClientOrderCode
        };

        using var document = await SendPostAsync("/shiip/public-api/v2/shipping-order/create", payload, cancellationToken);
        var data = GetDataElement(document);
        return new GhnCreateOrderResponseDto
        {
            OrderCode = ReadString(data, "order_code", "OrderCode", "orderCode")
        };
    }

    public async Task<GhnOrderInfoDto> GetOrderInfoAsync(string ghnOrderCode, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        using var document = await SendGetAsync($"/shiip/public-api/v2/shipping-order/detail?order_code={Uri.EscapeDataString(ghnOrderCode)}", cancellationToken);
        var data = GetDataElement(document);
        return new GhnOrderInfoDto
        {
            OrderCode = ReadString(data, "order_code", "OrderCode", "orderCode"),
            Status = ReadString(data, "status"),
            Note = ReadString(data, "note"),
            ExpectedDeliveryTime = ReadString(data, "leadtime", "expected_delivery_time", "ExpectedDeliveryTime"),
            TotalFee = ReadNullableInt(data, "total_fee", "TotalFee", "totalFee")
        };
    }

    public async Task CancelOrderAsync(string ghnOrderCode, string? reason, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        var payload = new Dictionary<string, object?>
        {
            ["order_code"] = ghnOrderCode,
            ["reason"] = reason
        };

        using var _ = await SendPostAsync("/shiip/public-api/v2/shipping-order/cancel", payload, cancellationToken);
    }

    public async Task<List<GhnServiceDto>> GetShippingServicesAsync(int fromDistrictId, int toDistrictId, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        var payload = new Dictionary<string, object?>
        {
            ["shop_id"] = _options.ShopId,
            ["from_district"] = fromDistrictId,
            ["to_district"] = toDistrictId
        };

        using var document = await SendPostAsync("/shiip/public-api/v2/shipping-order/available-services", payload, cancellationToken);
        return ReadList(document, item => new GhnServiceDto
        {
            ServiceId = ReadNullableInt(item, "service_id", "ServiceID", "ServiceId"),
            ServiceTypeId = ReadNullableInt(item, "service_type_id", "ServiceTypeId"),
            ShortName = ReadString(item, "short_name", "shortName", "ShortName")
        });
    }

    public async Task<string?> GetExpectedDeliveryTimeAsync(GhnExpectedDeliveryTimeRequestDto request, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        var payload = new Dictionary<string, object?>
        {
            ["from_district_id"] = request.FromDistrictId,
            ["to_district_id"] = request.ToDistrictId,
            ["service_id"] = request.ServiceId,
            ["to_ward_code"] = request.ToWardCode
        };

        using var document = await SendPostAsync("/shiip/public-api/v2/shipping-order/leadtime", payload, cancellationToken);
        var data = GetDataElement(document);
        return ReadString(data, "leadtime", "expected_delivery_time", "ExpectedDeliveryTime");
    }

    public async Task<GhnTrackingDto> TrackOrderAsync(string ghnOrderCode, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        using var document = await SendGetAsync($"/shiip/public-api/v2/shipping-order/tracking?order_code={Uri.EscapeDataString(ghnOrderCode)}", cancellationToken);
        var data = GetDataElement(document);

        var result = new GhnTrackingDto
        {
            OrderCode = ReadString(data, "order_code", "OrderCode", "orderCode") ?? ghnOrderCode,
            Status = ReadString(data, "status")
        };

        foreach (var key in new[] { "log", "logs", "timeline" })
        {
            if (TryGetProperty(data, key, out var logsElement) && logsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var log in logsElement.EnumerateArray())
                {
                    result.Logs.Add(new GhnTrackingEventDto
                    {
                        Status = ReadString(log, "status"),
                        Description = ReadString(log, "status_name", "description", "message"),
                        Time = ReadNullableDateTime(log, "updated_date", "updated_at", "time")
                    });
                }

                break;
            }
        }

        return result;
    }

    public Task<byte[]> PrintOrderAsync(string ghnOrderCode, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Encoding.UTF8.GetBytes($"GHN LABEL {ghnOrderCode}"));
    }

    public async Task UpdateOrderAsync(GhnUpdateOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        EnsureEnabled();
        var payload = new Dictionary<string, object?>
        {
            ["order_code"] = request.OrderCode,
            ["note"] = request.Note
        };

        using var _ = await SendPostAsync("/shiip/public-api/v2/shipping-order/update", payload, cancellationToken);
    }

    public async Task<string?> CreateGhnOrderAsync(Orders order, ShippingAddresses shippingAddress, CancellationToken cancellationToken = default)
    {
        var response = await CreateOrderAsync(new GhnCreateOrderRequestDto
        {
            ClientOrderCode = $"ORDER_{order.IdOrder}",
            ToName = shippingAddress.RecipientName,
            ToPhone = shippingAddress.PhoneNumber,
            ToAddress = shippingAddress.Address,
            ToDistrictId = shippingAddress.DistrictId,
            ToWardCode = shippingAddress.WardCode,
            FromDistrictId = _options.FromDistrictId,
            InsuranceValue = (int?)order.TotalAmount,
            CodAmount = string.Equals(order.PaymentMethod, "CASH", StringComparison.OrdinalIgnoreCase) ? (int?)order.FinalAmount : 0,
            Items = order.OrderDetails.Select(detail => new GhnOrderItemDto
            {
                Name = detail.ProductNameSnapshot,
                Code = detail.IdProduct.ToString(),
                Quantity = detail.Quantity,
                Price = (int)detail.Price
            }).ToList()
        }, cancellationToken);

        return response.OrderCode;
    }

    private void EnsureEnabled()
    {
        if (!IsEnabled())
        {
            throw new InvalidOperationException("GHN integration is disabled");
        }

        if (string.IsNullOrWhiteSpace(_options.BaseUrl) || string.IsNullOrWhiteSpace(_options.Token) || _options.ShopId <= 0)
        {
            throw new InvalidOperationException("GHN integration is not fully configured");
        }
    }

    private async Task<JsonDocument> SendGetAsync(string relativePath, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildUri(relativePath));
        AddHeaders(request);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ParseResponseAsync(response, cancellationToken);
    }

    private async Task<JsonDocument> SendPostAsync(string relativePath, object payload, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(relativePath))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        AddHeaders(request);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ParseResponseAsync(response, cancellationToken);
    }

    private async Task<JsonDocument> ParseResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(content, null, response.StatusCode);
        }

        var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(content) ? "{}" : content);
        var root = document.RootElement;
        if (TryGetProperty(root, "code", out var codeElement) && codeElement.ValueKind == JsonValueKind.Number)
        {
            var code = codeElement.GetInt32();
            if (code != 200)
            {
                var message = ReadString(root, "message") ?? "GHN request failed";
                document.Dispose();
                throw new InvalidOperationException(message);
            }
        }

        return document;
    }

    private Uri BuildUri(string relativePath)
    {
        var baseUri = _options.BaseUrl.TrimEnd('/');
        var path = relativePath.StartsWith('/') ? relativePath : "/" + relativePath;
        return new Uri(baseUri + path, UriKind.Absolute);
    }

    private void AddHeaders(HttpRequestMessage request)
    {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("Token", _options.Token);
        request.Headers.TryAddWithoutValidation("ShopId", _options.ShopId.ToString());
    }

    private static GhnCalculateFeeResponseDto DefaultFee() => new()
    {
        Total = 30000m,
        ServiceFee = 25000m,
        InsuranceFee = 0m,
        PickStationFee = 0m,
        CourierStationFee = 0m,
        CodFee = 0m,
        ReturnFee = 0m,
        R2sFee = 0m
    };

    private static JsonElement GetDataElement(JsonDocument document)
    {
        return TryGetProperty(document.RootElement, "data", out var data) ? data : document.RootElement;
    }

    private static List<T> ReadList<T>(JsonDocument document, Func<JsonElement, T> factory)
    {
        var result = new List<T>();
        var data = GetDataElement(document);
        if (data.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        foreach (var item in data.EnumerateArray())
        {
            result.Add(factory(item));
        }

        return result;
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static string? ReadString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetProperty(element, name, out var value) && value.ValueKind != JsonValueKind.Null)
            {
                return value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString(),
                    JsonValueKind.Number => value.GetRawText(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => value.GetRawText()
                };
            }
        }

        return null;
    }

    private static int ReadInt(JsonElement element, params string[] names)
    {
        return ReadNullableInt(element, names) ?? 0;
    }

    private static int? ReadNullableInt(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetProperty(element, name, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
                {
                    return number;
                }

                if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out number))
                {
                    return number;
                }
            }
        }

        return null;
    }

    private static decimal? ReadDecimal(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetProperty(element, name, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number))
                {
                    return number;
                }

                if (value.ValueKind == JsonValueKind.String && decimal.TryParse(value.GetString(), out number))
                {
                    return number;
                }
            }
        }

        return null;
    }

    private static DateTime? ReadNullableDateTime(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetProperty(element, name, out var value) && value.ValueKind == JsonValueKind.String && DateTime.TryParse(value.GetString(), out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }
}

public interface IPayOsService
{
    Task<PayOsPaymentResponseDto> CreatePaymentLinkAsync(Orders order, CancellationToken cancellationToken = default);
    bool VerifyWebhookSignature(string data, string? signature);
    Task<PayOsPaymentResponseDto> GetPaymentLinkInfoAsync(string paymentLinkId, CancellationToken cancellationToken = default);
    Task CancelPaymentLinkAsync(string paymentLinkId, CancellationToken cancellationToken = default);
}

public class PayOsService : IPayOsService
{
    private readonly PayOsOptions _options;

    public PayOsService(IOptions<PayOsOptions> options)
    {
        _options = options.Value;
    }

    public Task<PayOsPaymentResponseDto> CreatePaymentLinkAsync(Orders order, CancellationToken cancellationToken = default)
    {
        var paymentLinkId = Guid.NewGuid().ToString("N");
        return Task.FromResult(new PayOsPaymentResponseDto
        {
            Code = "00",
            Desc = "success",
            Data = new PayOsPaymentDataDto
            {
                PaymentLinkId = paymentLinkId,
                CheckoutUrl = $"{_options.ReturnUrl}?orderId={order.IdOrder}&paymentLinkId={paymentLinkId}",
                QrCode = null,
                OrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Amount = order.FinalAmount ?? order.TotalAmount,
                Description = $"Thanh toan don hang #{order.IdOrder}",
                Status = "PENDING"
            }
        });
    }

    public bool VerifyWebhookSignature(string data, string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(_options.ChecksumKey))
        {
            return true;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.ChecksumKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        var computed = Convert.ToHexString(hash).ToLowerInvariant();
        return string.Equals(computed, signature.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public Task<PayOsPaymentResponseDto> GetPaymentLinkInfoAsync(string paymentLinkId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PayOsPaymentResponseDto
        {
            Code = "00",
            Desc = "success",
            Data = new PayOsPaymentDataDto
            {
                PaymentLinkId = paymentLinkId,
                Status = "PENDING"
            }
        });
    }

    public Task CancelPaymentLinkAsync(string paymentLinkId, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public interface IShipmentService
{
    Task<ShipmentDto> GetShipmentByIdAsync(int shipmentId, CancellationToken cancellationToken = default);
    Task<ShipmentDto> GetShipmentByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<GhnTrackingDto> GetShipmentTrackingAsync(int shipmentId, CancellationToken cancellationToken = default);
    Task<ShipmentDto> SyncWithGhnAsync(int shipmentId, CancellationToken cancellationToken = default);
    Task<ShipmentDto> CreateGhnShipmentForOrderAsync(int orderId, CancellationToken cancellationToken = default);
}

public class ShipmentService : IShipmentService
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IGhnService _ghnService;
    private readonly ISystemSettingService _systemSettingService;

    public ShipmentService(IShipmentRepository shipmentRepository, IOrderRepository orderRepository, IGhnService ghnService, ISystemSettingService systemSettingService)
    {
        _shipmentRepository = shipmentRepository;
        _orderRepository = orderRepository;
        _ghnService = ghnService;
        _systemSettingService = systemSettingService;
    }

    public async Task<ShipmentDto> GetShipmentByIdAsync(int shipmentId, CancellationToken cancellationToken = default)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(shipmentId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Shipment khong ton tai voi ID: {shipmentId}");
        return Map(shipment);
    }

    public async Task<ShipmentDto> GetShipmentByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var shipment = await _shipmentRepository.GetByOrderIdAsync(orderId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Shipment khong ton tai cho order ID: {orderId}");
        return Map(shipment);
    }

    public async Task<GhnTrackingDto> GetShipmentTrackingAsync(int shipmentId, CancellationToken cancellationToken = default)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(shipmentId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Shipment khong ton tai voi ID: {shipmentId}");
        var orderCode = shipment.GhnOrderCode ?? shipment.TrackingNumber ?? throw new InvalidOperationException("Shipment khong co GHN order code");
        return await _ghnService.TrackOrderAsync(orderCode, cancellationToken);
    }

    public async Task<ShipmentDto> SyncWithGhnAsync(int shipmentId, CancellationToken cancellationToken = default)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(shipmentId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Shipment khong ton tai voi ID: {shipmentId}");
        var orderCode = shipment.GhnOrderCode ?? shipment.TrackingNumber ?? throw new InvalidOperationException("Shipment khong co GHN order code");
        var info = await _ghnService.GetOrderInfoAsync(orderCode, cancellationToken);
        shipment.GhnStatus = info.Status;
        shipment.GhnUpdatedAt = DateTime.UtcNow;
        shipment.GhnNote = info.Note;
        shipment.GhnShippingFee = info.TotalFee;
        if (string.Equals(info.Status, "delivered", StringComparison.OrdinalIgnoreCase))
        {
            shipment.ShippingStatus = "DELIVERED";
            if (shipment.IdOrderNavigation.Status != "COMPLETED")
            {
                shipment.IdOrderNavigation.Status = "COMPLETED";
                shipment.IdOrderNavigation.CompletedAt = DateTime.UtcNow;
                shipment.IdOrderNavigation.DeliveredAt = DateTime.UtcNow;
                shipment.IdOrderNavigation.ReturnWindowDays = await _systemSettingService.GetReturnWindowDaysAsync(cancellationToken);
            }
        }
        await _shipmentRepository.SaveChangesAsync(cancellationToken);
        return Map(shipment);
    }

    public async Task<ShipmentDto> CreateGhnShipmentForOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Order khong ton tai voi ID: {orderId}");
        if (order.IdShippingAddressNavigation is null)
        {
            throw new InvalidOperationException("Don hang khong co dia chi giao hang");
        }

        var shipment = await _shipmentRepository.GetByOrderIdAsync(orderId, cancellationToken)
            ?? new Shipments
            {
                IdOrder = orderId,
                ShippingMethod = "GHN",
                ShippingStatus = "PREPARING"
            };

        if (shipment.IdShipment == 0)
        {
            await _shipmentRepository.AddAsync(shipment, cancellationToken);
        }

        var orderCode = await _ghnService.CreateGhnOrderAsync(order, order.IdShippingAddressNavigation, cancellationToken);
        shipment.GhnOrderCode = orderCode;
        shipment.TrackingNumber = orderCode;
        shipment.GhnUpdatedAt = DateTime.UtcNow;
        shipment.GhnStatus = "ready_to_pick";
        shipment.ShippingStatus = "PREPARING";
        await _shipmentRepository.SaveChangesAsync(cancellationToken);
        return Map(shipment);
    }

    private static ShipmentDto Map(Shipments shipment)
    {
        return new ShipmentDto
        {
            IdShipment = shipment.IdShipment,
            IdOrder = shipment.IdOrder,
            ShippingStatus = shipment.ShippingStatus,
            TrackingNumber = shipment.TrackingNumber,
            LocationLat = shipment.LocationLat,
            LocationLong = shipment.LocationLong,
            CreatedAt = shipment.CreatedAt,
            UpdatedAt = shipment.UpdatedAt,
            GhnOrderCode = shipment.GhnOrderCode,
            GhnShippingFee = shipment.GhnShippingFee,
            GhnExpectedDeliveryTime = shipment.GhnExpectedDeliveryTime,
            GhnStatus = shipment.GhnStatus,
            GhnUpdatedAt = shipment.GhnUpdatedAt,
            GhnNote = shipment.GhnNote,
            ShippingMethod = shipment.ShippingMethod
        };
    }
}