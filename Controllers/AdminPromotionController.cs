using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Promotion;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/admin")]
public class AdminPromotionController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public AdminPromotionController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [HttpPost("promotions")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PromotionDto>>> CreatePromotion([FromBody] PromotionDto dto, CancellationToken cancellationToken)
    {
        var result = await _promotionService.CreatePromotionAsync(dto, cancellationToken);
        return Ok(ApiResponse<PromotionDto>.Success("Tạo mã giảm giá thành công", result));
    }

    [HttpGet("promotions")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<PromotionDto>>>> GetAllPromotions([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "CreatedAt", [FromQuery] string sortDirection = "DESC", [FromQuery] string? keyword = null, [FromQuery] string? scope = null, CancellationToken cancellationToken = default)
    {
        var result = await _promotionService.GetAllPromotionsAsync(keyword, scope, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<PromotionDto>>.Success("Lấy danh sách mã giảm giá thành công", result));
    }

    [HttpGet("promotions/{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PromotionDto>>> GetPromotionById(int id, CancellationToken cancellationToken)
    {
        var result = await _promotionService.GetPromotionByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PromotionDto>.Success("Lấy thông tin mã giảm giá thành công", result));
    }

    [HttpPut("promotions/{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PromotionDto>>> UpdatePromotion(int id, [FromBody] PromotionDto dto, CancellationToken cancellationToken)
    {
        var result = await _promotionService.UpdatePromotionAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<PromotionDto>.Success("Cập nhật mã giảm giá thành công", result));
    }

    [HttpDelete("promotions/{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<object>>> DeletePromotion(int id, CancellationToken cancellationToken)
    {
        await _promotionService.DeletePromotionAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Success("Xóa mã giảm giá thành công", null));
    }

    [HttpPost("promotion-rules")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PromotionRuleDto>>> CreatePromotionRule([FromBody] PromotionRuleDto dto, CancellationToken cancellationToken)
    {
        var result = await _promotionService.CreatePromotionRuleAsync(dto, cancellationToken);
        return Ok(ApiResponse<PromotionRuleDto>.Success("Tạo quy tắc giảm giá thành công", result));
    }

    [HttpGet("promotion-rules")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<PromotionRuleDto>>>> GetAllPromotionRules([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "CreatedAt", [FromQuery] string sortDirection = "DESC", CancellationToken cancellationToken = default)
    {
        var result = await _promotionService.GetAllPromotionRulesAsync(pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<PromotionRuleDto>>.Success("Lấy danh sách quy tắc giảm giá thành công", result));
    }

    [HttpGet("promotion-rules/{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PromotionRuleDto>>> GetPromotionRuleById(int id, CancellationToken cancellationToken)
    {
        var result = await _promotionService.GetPromotionRuleByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PromotionRuleDto>.Success("Lấy thông tin quy tắc giảm giá thành công", result));
    }

    [HttpPut("promotion-rules/{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PromotionRuleDto>>> UpdatePromotionRule(int id, [FromBody] PromotionRuleDto dto, CancellationToken cancellationToken)
    {
        var result = await _promotionService.UpdatePromotionRuleAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<PromotionRuleDto>.Success("Cập nhật quy tắc giảm giá thành công", result));
    }

    [HttpDelete("promotion-rules/{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<object>>> DeletePromotionRule(int id, CancellationToken cancellationToken)
    {
        await _promotionService.DeletePromotionRuleAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Success("Xóa quy tắc giảm giá thành công", null));
    }
}