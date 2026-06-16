using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Shipment;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/shipping-addresses")]
public class ShippingAddressController : ControllerBase
{
    private readonly IShippingAddressService _shippingAddressService;
    private readonly ICustomerService _customerService;

    public ShippingAddressController(IShippingAddressService shippingAddressService, ICustomerService customerService)
    {
        _shippingAddressService = shippingAddressService;
        _customerService = customerService;
    }

    [HttpGet]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<List<ShippingAddressDto>>>> GetAllAddresses(CancellationToken cancellationToken)
    {
        var customerId = await GetCurrentCustomerIdAsync(cancellationToken);
        var addresses = await _shippingAddressService.GetAllAddressesAsync(customerId, cancellationToken);
        return Ok(ApiResponse<List<ShippingAddressDto>>.Success("Lấy danh sách địa chỉ thành công", addresses));
    }

    [HttpGet("default")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<ShippingAddressDto>>> GetDefaultAddress(CancellationToken cancellationToken)
    {
        var customerId = await GetCurrentCustomerIdAsync(cancellationToken);
        var address = await _shippingAddressService.GetDefaultAddressAsync(customerId, cancellationToken);
        return Ok(ApiResponse<ShippingAddressDto>.Success("Lấy địa chỉ mặc định thành công", address));
    }

    [HttpPost]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<ShippingAddressDto>>> CreateAddress([FromBody] CreateShippingAddressRequestDto request, CancellationToken cancellationToken)
    {
        var customerId = await GetCurrentCustomerIdAsync(cancellationToken);
        var address = await _shippingAddressService.CreateAddressAsync(customerId, request, cancellationToken);
        return Ok(ApiResponse<ShippingAddressDto>.Success("Tạo địa chỉ thành công", address));
    }

    [HttpPut("{addressId:int}")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<ShippingAddressDto>>> UpdateAddress(int addressId, [FromBody] UpdateShippingAddressRequestDto request, CancellationToken cancellationToken)
    {
        var customerId = await GetCurrentCustomerIdAsync(cancellationToken);
        var address = await _shippingAddressService.UpdateAddressAsync(customerId, addressId, request, cancellationToken);
        return Ok(ApiResponse<ShippingAddressDto>.Success("Cập nhật địa chỉ thành công", address));
    }

    [HttpPut("{addressId:int}/set-default")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<ShippingAddressDto>>> SetDefaultAddress(int addressId, CancellationToken cancellationToken)
    {
        var customerId = await GetCurrentCustomerIdAsync(cancellationToken);
        var address = await _shippingAddressService.SetDefaultAddressAsync(customerId, addressId, cancellationToken);
        return Ok(ApiResponse<ShippingAddressDto>.Success("Đặt địa chỉ mặc định thành công", address));
    }

    [HttpDelete("{addressId:int}")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAddress(int addressId, CancellationToken cancellationToken)
    {
        var customerId = await GetCurrentCustomerIdAsync(cancellationToken);
        await _shippingAddressService.DeleteAddressAsync(customerId, addressId, cancellationToken);
        return Ok(ApiResponse<object>.Success("Xóa địa chỉ thành công", null));
    }

    private async Task<int> GetCurrentCustomerIdAsync(CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("Yêu cầu đăng nhập");
        var customer = await _customerService.GetCustomerByUsernameAsync(username, cancellationToken);
        return customer.IdCustomer ?? throw new InvalidOperationException("Customer không hợp lệ");
    }
}
