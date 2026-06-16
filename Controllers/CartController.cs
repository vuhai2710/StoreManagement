using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Cart;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ICustomerService _customerService;

    public CartController(ICartService cartService, ICustomerService customerService)
    {
        _cartService = cartService;
        _customerService = customerService;
    }

    [HttpGet]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<CartDto>>> GetCart(CancellationToken cancellationToken)
    {
        var customerId = await GetCurrentCustomerIdAsync(cancellationToken);
        var cart = await _cartService.GetCartAsync(customerId, cancellationToken);
        return Ok(ApiResponse<CartDto>.Success("Lấy giỏ hàng thành công", cart));
    }

    [HttpPost("items")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<CartDto>>> AddToCart([FromBody] AddToCartRequestDto request, CancellationToken cancellationToken)
    {
        var customerId = await GetCurrentCustomerIdAsync(cancellationToken);
        var cart = await _cartService.AddToCartAsync(customerId, request, cancellationToken);
        return Ok(ApiResponse<CartDto>.Success("Thêm sản phẩm vào giỏ hàng thành công", cart));
    }

    [HttpPut("items/{itemId:int}")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<CartDto>>> UpdateCartItem(int itemId, [FromBody] UpdateCartItemRequestDto request, CancellationToken cancellationToken)
    {
        var customerId = await GetCurrentCustomerIdAsync(cancellationToken);
        var cart = await _cartService.UpdateCartItemAsync(customerId, itemId, request, cancellationToken);
        return Ok(ApiResponse<CartDto>.Success("Cập nhật giỏ hàng thành công", cart));
    }

    [HttpDelete("items/{itemId:int}")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<CartDto>>> RemoveCartItem(int itemId, CancellationToken cancellationToken)
    {
        var customerId = await GetCurrentCustomerIdAsync(cancellationToken);
        var cart = await _cartService.RemoveCartItemAsync(customerId, itemId, cancellationToken);
        return Ok(ApiResponse<CartDto>.Success("Xóa sản phẩm khỏi giỏ hàng thành công", cart));
    }

    [HttpDelete]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<object>>> ClearCart(CancellationToken cancellationToken)
    {
        var customerId = await GetCurrentCustomerIdAsync(cancellationToken);
        await _cartService.ClearCartAsync(customerId, cancellationToken);
        return Ok(ApiResponse<object>.Success("Xóa giỏ hàng thành công", null));
    }

    private async Task<int> GetCurrentCustomerIdAsync(CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("Yêu cầu đăng nhập");
        var customer = await _customerService.GetCustomerByUsernameAsync(username, cancellationToken);
        return customer.IdCustomer ?? throw new InvalidOperationException("Customer không hợp lệ");
    }
}
