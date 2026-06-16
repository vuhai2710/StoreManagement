using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Promotion;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/promotions")]
public class PromotionController : ControllerBase
{
    private readonly IPromotionService _promotionService;
    private readonly ICustomerService _customerService;

    public PromotionController(IPromotionService promotionService, ICustomerService customerService)
    {
        _promotionService = promotionService;
        _customerService = customerService;
    }

    [HttpPost("validate")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<ValidatePromotionResponseDto>>> ValidatePromotion([FromBody] ValidatePromotionRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _promotionService.ValidatePromotionAsync(request, cancellationToken);
        return Ok(ApiResponse<ValidatePromotionResponseDto>.Success("Validate promotion code", response));
    }

    [HttpPost("calculate")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<CalculateDiscountResponseDto>>> CalculateDiscount([FromBody] CalculateDiscountRequestDto request, CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("Yêu cầu đăng nhập");
        var customer = await _customerService.GetCustomerByUsernameAsync(username, cancellationToken);
        var customerType = customer.CustomerType ?? "REGULAR";
        var response = await _promotionService.CalculateAutomaticDiscountAsync(request, customerType, cancellationToken);
        return Ok(ApiResponse<CalculateDiscountResponseDto>.Success("Calculate automatic discount", response));
    }

    [HttpPost("calculate-auto-shipping")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<CalculateDiscountResponseDto>>> CalculateAutoShipping([FromBody] CalculateDiscountRequestDto request, CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("Yêu cầu đăng nhập");
        var customer = await _customerService.GetCustomerByUsernameAsync(username, cancellationToken);
        var customerType = customer.CustomerType ?? "REGULAR";
        var response = await _promotionService.CalculateAutoShippingDiscountAsync(request.ShippingFee, request.TotalAmount, customerType, cancellationToken);
        return Ok(ApiResponse<CalculateDiscountResponseDto>.Success("Calculate automatic shipping discount", response));
    }
}
