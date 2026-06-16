using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Order;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly IEmployeeService _employeeService;
    private readonly ICurrentUserContext _currentUserContext;

    public OrderController(IOrderService orderService, ICustomerService customerService, IEmployeeService employeeService, ICurrentUserContext currentUserContext)
    {
        _orderService = orderService;
        _customerService = customerService;
        _employeeService = employeeService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrderById(int id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<OrderDto>.Success("Lấy thông tin đơn hàng thành công", order));
    }

    [HttpGet("{id:int}/pdf")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<IActionResult> ExportOrderToPdf(int id, CancellationToken cancellationToken)
    {
        var bytes = await _orderService.ExportOrderToPdfAsync(id, cancellationToken);
        return File(bytes, "application/pdf", $"hoa-don-{id}.pdf");
    }

    [HttpPost("checkout")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Checkout([FromBody] OrderDto request, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetCustomerByUsernameAsync(_currentUserContext.GetRequiredUsername(), cancellationToken);
        var order = await _orderService.CreateOrderFromCartAsync(customer.IdCustomer!.Value, request, cancellationToken);
        return Ok(ApiResponse<OrderDto>.Success("Đặt hàng thành công", order));
    }

    [HttpPost("buy-now")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> BuyNow([FromBody] OrderDto request, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetCustomerByUsernameAsync(_currentUserContext.GetRequiredUsername(), cancellationToken);
        var order = await _orderService.CreateOrderDirectlyAsync(customer.IdCustomer!.Value, request, cancellationToken);
        return Ok(ApiResponse<OrderDto>.Success("Đặt hàng thành công", order));
    }

    [HttpPost("create-for-customer")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrderForCustomer([FromBody] OrderDto request, CancellationToken cancellationToken)
    {
        var employeeId = _currentUserContext.IsInRole("EMPLOYEE") ? _currentUserContext.GetEmployeeId() : null;
        var order = await _orderService.CreateOrderForCustomerAsync(employeeId, request, cancellationToken);
        return Ok(ApiResponse<OrderDto>.Success("Tạo đơn hàng thành công", order));
    }

    [HttpGet("my-orders")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<PageResponse<OrderDto>>>> GetMyOrders([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "orderDate", [FromQuery] string sortDirection = "DESC", [FromQuery] string? status = null, [FromQuery] string? keyword = null, CancellationToken cancellationToken = default)
    {
        var customer = await _customerService.GetCustomerByUsernameAsync(_currentUserContext.GetRequiredUsername(), cancellationToken);
        var result = await _orderService.GetMyOrdersAsync(customer.IdCustomer!.Value, status, keyword, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<OrderDto>>.Success("Lấy danh sách đơn hàng thành công", result));
    }

    [HttpGet("my-orders/{orderId:int}")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetMyOrderById(int orderId, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetCustomerByUsernameAsync(_currentUserContext.GetRequiredUsername(), cancellationToken);
        var result = await _orderService.GetMyOrderByIdAsync(customer.IdCustomer!.Value, orderId, cancellationToken);
        return Ok(ApiResponse<OrderDto>.Success("Lấy thông tin đơn hàng thành công", result));
    }

    [HttpPut("my-orders/{orderId:int}/cancel")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CancelOrder(int orderId, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetCustomerByUsernameAsync(_currentUserContext.GetRequiredUsername(), cancellationToken);
        var result = await _orderService.CancelOrderAsync(customer.IdCustomer!.Value, orderId, cancellationToken);
        return Ok(ApiResponse<OrderDto>.Success("Hủy đơn hàng thành công", result));
    }

    [HttpPut("my-orders/{orderId:int}/confirm-delivery")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> ConfirmDelivery(int orderId, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetCustomerByUsernameAsync(_currentUserContext.GetRequiredUsername(), cancellationToken);
        var result = await _orderService.ConfirmDeliveryAsync(customer.IdCustomer!.Value, orderId, cancellationToken);
        return Ok(ApiResponse<OrderDto>.Success("Xác nhận nhận hàng thành công", result));
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<OrderDto>>>> GetAllOrders([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "orderDate", [FromQuery] string sortDirection = "DESC", [FromQuery] string? status = null, [FromQuery] int? customerId = null, [FromQuery] string? keyword = null, CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetAllOrdersAsync(status, customerId, keyword, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<OrderDto>>.Success("Lấy danh sách đơn hàng thành công", result));
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateOrderStatus(int id, [FromBody] OrderDto request, CancellationToken cancellationToken)
    {
        var result = await _orderService.UpdateOrderStatusAsync(id, request.Status ?? string.Empty, cancellationToken);
        return Ok(ApiResponse<OrderDto>.Success("Cập nhật trạng thái đơn hàng thành công", result));
    }
}