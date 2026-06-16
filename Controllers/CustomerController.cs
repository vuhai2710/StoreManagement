using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Auth;
using StoreManagement.Dtos.Customer;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/customers")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IUserService _userService;

    public CustomerController(ICustomerService customerService, IUserService userService)
    {
        _customerService = customerService;
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<CustomerDto>>>> GetAllCustomers(
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 5,
        [FromQuery] string sortBy = "IdCustomer",
        [FromQuery] string sortDirection = "ASC",
        [FromQuery] string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        var page = await _customerService.GetAllCustomersPaginatedAsync(keyword, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<CustomerDto>>.Success("Lấy danh sách customer thành công", page));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomerById(int id, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<CustomerDto>.Success("Lấy thông tin customer thành công", customer));
    }

    [HttpGet("search")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<CustomerDto>>>> SearchCustomers(
        [FromQuery] string? name = null,
        [FromQuery] string? phone = null,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 5,
        [FromQuery] string sortBy = "IdCustomer",
        [FromQuery] string sortDirection = "ASC",
        CancellationToken cancellationToken = default)
    {
        var page = await _customerService.SearchCustomersAsync(name, phone, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<CustomerDto>>.Success("Tìm kiếm customer thành công", page));
    }

    [HttpGet("type/{type}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<CustomerDto>>>> GetCustomersByType(
        string type,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 5,
        [FromQuery] string sortBy = "IdCustomer",
        [FromQuery] string sortDirection = "ASC",
        CancellationToken cancellationToken = default)
    {
        var page = await _customerService.GetCustomersByTypeAsync(type, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<CustomerDto>>.Success("Lấy danh sách customer theo loại thành công", page));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> UpdateCustomer(int id, [FromBody] CustomerDto customerDto, CancellationToken cancellationToken)
    {
        var updated = await _customerService.UpdateCustomerAsync(id, customerDto, cancellationToken);
        return Ok(ApiResponse<CustomerDto>.Success("Cập nhật customer thành công", updated));
    }

    [HttpPatch("{id:int}/upgrade-vip")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> UpgradeToVip(int id, CancellationToken cancellationToken)
    {
        var customer = await _customerService.UpgradeToVipAsync(id, cancellationToken);
        return Ok(ApiResponse<CustomerDto>.Success("Nâng cấp customer lên VIP thành công", customer));
    }

    [HttpPatch("{id:int}/downgrade-regular")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> DowngradeToRegular(int id, CancellationToken cancellationToken)
    {
        var customer = await _customerService.DowngradeToRegularAsync(id, cancellationToken);
        return Ok(ApiResponse<CustomerDto>.Success("Hạ cấp customer xuống REGULAR thành công", customer));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCustomer(int id, CancellationToken cancellationToken)
    {
        await _customerService.DeleteCustomerAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Success("Xóa customer thành công", null));
    }

    [HttpGet("me")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetMyInfo(CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("Yêu cầu đăng nhập");
        var customer = await _customerService.GetCustomerByUsernameAsync(username, cancellationToken);
        return Ok(ApiResponse<CustomerDto>.Success("Lấy thông tin thành công", customer));
    }

    [HttpPut("me")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> UpdateMyInfo([FromBody] CustomerDto customerDto, CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("Yêu cầu đăng nhập");
        var customer = await _customerService.UpdateMyCustomerInfoAsync(username, customerDto, cancellationToken);
        return Ok(ApiResponse<CustomerDto>.Success("Cập nhật thông tin thành công", customer));
    }

    [HttpPut("me/change-password")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordDto request, CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("Yêu cầu đăng nhập");
        await _userService.ChangePasswordAsync(username, request.CurrentPassword, request.NewPassword, cancellationToken);
        return Ok(ApiResponse<object>.Success("Đổi mật khẩu thành công", null));
    }
}
