using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/settings")]
public class SystemSettingController : ControllerBase
{
    private readonly ISystemSettingService _systemSettingService;

    public SystemSettingController(ISystemSettingService systemSettingService)
    {
        _systemSettingService = systemSettingService;
    }

    [HttpGet("return-window")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> GetReturnWindow(CancellationToken cancellationToken)
    {
        var days = await _systemSettingService.GetReturnWindowDaysAsync(cancellationToken);
        return Ok(ApiResponse<Dictionary<string, object>>.Success("Lấy cấu hình thành công", new Dictionary<string, object>
        {
            ["days"] = days
        }));
    }

    [HttpPut("return-window")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> UpdateReturnWindow([FromQuery] int days, CancellationToken cancellationToken)
    {
        await _systemSettingService.UpdateReturnWindowAsync(days, cancellationToken);
        return Ok(ApiResponse<Dictionary<string, object>>.Success("Cập nhật thành công", new Dictionary<string, object>
        {
            ["message"] = "Cập nhật thành công",
            ["days"] = days
        }));
    }

    [HttpGet("homepage-policy")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object?>>>> GetHomepagePolicy(CancellationToken cancellationToken)
    {
        var returnWindowDays = await _systemSettingService.GetReturnWindowDaysAsync(cancellationToken);
        var autoFreeShippingPromotion = await _systemSettingService.GetAutoFreeShippingPromotionAsync(cancellationToken);
        return Ok(ApiResponse<Dictionary<string, object?>>.Success("Lấy cấu hình thành công", new Dictionary<string, object?>
        {
            ["returnWindowDays"] = returnWindowDays,
            ["autoFreeShippingPromotion"] = autoFreeShippingPromotion
        }));
    }

    [HttpGet("auto-free-shipping-promotion")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object?>>>> GetAutoFreeShippingPromotion(CancellationToken cancellationToken)
    {
        var code = await _systemSettingService.GetAutoFreeShippingPromotionAsync(cancellationToken);
        return Ok(ApiResponse<Dictionary<string, object?>>.Success("Lấy cấu hình thành công", new Dictionary<string, object?>
        {
            ["code"] = code
        }));
    }

    [HttpPut("auto-free-shipping-promotion")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object?>>>> UpdateAutoFreeShippingPromotion([FromQuery] string? code, CancellationToken cancellationToken)
    {
        await _systemSettingService.UpdateAutoFreeShippingPromotionAsync(code, cancellationToken);
        return Ok(ApiResponse<Dictionary<string, object?>>.Success("Cập nhật thành công", new Dictionary<string, object?>
        {
            ["message"] = "Cập nhật thành công",
            ["code"] = await _systemSettingService.GetAutoFreeShippingPromotionAsync(cancellationToken)
        }));
    }

    [HttpGet("review-edit-window")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> GetReviewEditWindow(CancellationToken cancellationToken)
    {
        var hours = await _systemSettingService.GetReviewEditWindowHoursAsync(cancellationToken);
        return Ok(ApiResponse<Dictionary<string, object>>.Success("Lấy cấu hình thành công", new Dictionary<string, object>
        {
            ["hours"] = hours
        }));
    }

    [HttpPut("review-edit-window")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> UpdateReviewEditWindow([FromQuery] int hours, CancellationToken cancellationToken)
    {
        await _systemSettingService.UpdateReviewEditWindowAsync(hours, cancellationToken);
        return Ok(ApiResponse<Dictionary<string, object>>.Success("Cập nhật thành công", new Dictionary<string, object>
        {
            ["message"] = "Cập nhật thành công",
            ["hours"] = hours
        }));
    }
}
