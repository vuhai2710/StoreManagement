using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Order;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/order-returns")]
public class OrderReturnController : ControllerBase
{
    private readonly IOrderReturnService _orderReturnService;
    private readonly ICustomerService _customerService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ISystemSettingService _systemSettingService;

    public OrderReturnController(IOrderReturnService orderReturnService, ICustomerService customerService, ICurrentUserContext currentUserContext, ISystemSettingService systemSettingService)
    {
        _orderReturnService = orderReturnService;
        _customerService = customerService;
        _currentUserContext = currentUserContext;
        _systemSettingService = systemSettingService;
    }

    [HttpGet("config")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> GetReturnConfig(CancellationToken cancellationToken)
    {
        var allowedDays = await _systemSettingService.GetReturnWindowDaysAsync(cancellationToken);
        return Ok(ApiResponse<Dictionary<string, object>>.Success("Lấy cấu hình thành công", new Dictionary<string, object>
        {
            ["allowedDays"] = allowedDays
        }));
    }

    [HttpPost("orders/{orderId:int}/return")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<OrderReturnDto>>> RequestReturn(int orderId, [FromBody] OrderReturnDto request, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetCustomerByUsernameAsync(_currentUserContext.GetRequiredUsername(), cancellationToken);
        var result = await _orderReturnService.RequestReturnAsync(customer.IdCustomer!.Value, orderId, request, cancellationToken);
        return Ok(ApiResponse<OrderReturnDto>.Success("Tạo yêu cầu trả hàng thành công", result));
    }

    [HttpPost("orders/{orderId:int}/exchange")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<OrderReturnDto>>> RequestExchange(int orderId, [FromBody] OrderReturnDto request, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetCustomerByUsernameAsync(_currentUserContext.GetRequiredUsername(), cancellationToken);
        var result = await _orderReturnService.RequestExchangeAsync(customer.IdCustomer!.Value, orderId, request, cancellationToken);
        return Ok(ApiResponse<OrderReturnDto>.Success("Tạo yêu cầu đổi hàng thành công", result));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "CUSTOMER,ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<OrderReturnDto>>> GetOrderReturnById(int id, CancellationToken cancellationToken)
    {
        var result = await _orderReturnService.GetOrderReturnByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<OrderReturnDto>.Success("Lấy thông tin yêu cầu đổi/trả thành công", result));
    }

    [HttpGet("my-returns")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<PageResponse<OrderReturnDto>>>> GetMyReturns([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "createdAt", [FromQuery] string sortDirection = "DESC", CancellationToken cancellationToken = default)
    {
        var customer = await _customerService.GetCustomerByUsernameAsync(_currentUserContext.GetRequiredUsername(), cancellationToken);
        var result = await _orderReturnService.GetMyReturnsAsync(customer.IdCustomer!.Value, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<OrderReturnDto>>.Success("Lấy danh sách yêu cầu đổi/trả thành công", result));
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<OrderReturnDto>>>> GetAllReturns([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "createdAt", [FromQuery] string sortDirection = "DESC", [FromQuery] string? status = null, [FromQuery] string? returnType = null, [FromQuery] string? keyword = null, [FromQuery] string? customerKeyword = null, CancellationToken cancellationToken = default)
    {
        var result = await _orderReturnService.GetAllReturnsAsync(pageNo, pageSize, sortBy, sortDirection, status, returnType, keyword, customerKeyword, cancellationToken);
        return Ok(ApiResponse<PageResponse<OrderReturnDto>>.Success("Lấy danh sách yêu cầu đổi/trả thành công", result));
    }

    [HttpGet("orders/{orderId:int}/has-active")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<bool>>> HasActiveReturnRequest(int orderId, CancellationToken cancellationToken)
    {
        var result = await _orderReturnService.HasActiveReturnRequestAsync(orderId, cancellationToken);
        return Ok(ApiResponse<bool>.Success("Kiểm tra yêu cầu đổi/trả", result));
    }

    [HttpPut("{idReturn:int}/approve")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<OrderReturnDto>>> Approve(int idReturn, [FromBody] OrderReturnDto request, CancellationToken cancellationToken)
    {
        var result = await _orderReturnService.ApproveAsync(idReturn, _currentUserContext.GetEmployeeId(), request.NoteAdmin, request.RefundAmount, cancellationToken);
        return Ok(ApiResponse<OrderReturnDto>.Success("Duyệt yêu cầu đổi/trả thành công", result));
    }

    [HttpPut("{idReturn:int}/reject")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<OrderReturnDto>>> Reject(int idReturn, [FromBody] OrderReturnDto request, CancellationToken cancellationToken)
    {
        var result = await _orderReturnService.RejectAsync(idReturn, _currentUserContext.GetEmployeeId(), request.NoteAdmin, cancellationToken);
        return Ok(ApiResponse<OrderReturnDto>.Success("Từ chối yêu cầu đổi/trả thành công", result));
    }

    [HttpPut("{idReturn:int}/complete")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<OrderReturnDto>>> Complete(int idReturn, CancellationToken cancellationToken)
    {
        var result = await _orderReturnService.CompleteAsync(idReturn, _currentUserContext.GetEmployeeId(), cancellationToken);
        return Ok(ApiResponse<OrderReturnDto>.Success("Hoàn tất yêu cầu đổi/trả thành công", result));
    }
}