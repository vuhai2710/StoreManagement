using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Reporting;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/admin/dashboard")]
[Authorize(Roles = "ADMIN,EMPLOYEE")]
public class AdminDashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public AdminDashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<DashboardOverviewDto>>> GetDashboardOverview([FromQuery] int topProducts = 5, [FromQuery] int recentOrders = 10, [FromQuery] int chartDays = 7, CancellationToken cancellationToken = default)
    {
        topProducts = Math.Clamp(topProducts, 1, 20);
        recentOrders = Math.Clamp(recentOrders, 1, 50);
        chartDays = Math.Clamp(chartDays, 1, 90);
        var result = await _dashboardService.GetDashboardOverviewAsync(topProducts, recentOrders, chartDays, cancellationToken);
        return Ok(ApiResponse<DashboardOverviewDto>.Success("Lấy dữ liệu dashboard thành công", result));
    }
}

[ApiController]
[Route("api/v1/admin/reports")]
[Authorize(Roles = "ADMIN,EMPLOYEE")]
public class AdminReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public AdminReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("revenue-summary")]
    public async Task<ActionResult<ApiResponse<RevenueSummaryDto>>> GetRevenueSummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, CancellationToken cancellationToken)
    {
        if (fromDate > toDate)
        {
            return BadRequest(ApiResponse<RevenueSummaryDto>.Error(400, "Ngày bắt đầu phải trước ngày kết thúc"));
        }

        var result = await _reportService.GetRevenueSummaryAsync(fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<RevenueSummaryDto>.Success("Lấy báo cáo doanh thu thành công", result));
    }

    [HttpGet("revenue-by-time")]
    public async Task<ActionResult<ApiResponse<List<RevenueByTimeDto>>>> GetRevenueByTime([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] string groupBy = "MONTH", CancellationToken cancellationToken = default)
    {
        if (fromDate > toDate)
        {
            return BadRequest(ApiResponse<List<RevenueByTimeDto>>.Error(400, "Ngày bắt đầu phải trước ngày kết thúc"));
        }

        var normalized = groupBy.ToUpperInvariant();
        if (normalized is not ("DAY" or "MONTH" or "YEAR"))
        {
            return BadRequest(ApiResponse<List<RevenueByTimeDto>>.Error(400, "Tham số groupBy không hợp lệ. Chấp nhận: DAY, MONTH, YEAR"));
        }

        var result = await _reportService.GetRevenueByTimeAsync(fromDate, toDate, normalized, cancellationToken);
        return Ok(ApiResponse<List<RevenueByTimeDto>>.Success("Lấy dữ liệu biểu đồ doanh thu thành công", result));
    }

    [HttpGet("revenue-by-product")]
    public async Task<ActionResult<ApiResponse<List<RevenueByProductDto>>>> GetRevenueByProduct([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] int limit = 20, CancellationToken cancellationToken = default)
    {
        if (fromDate > toDate)
        {
            return BadRequest(ApiResponse<List<RevenueByProductDto>>.Error(400, "Ngày bắt đầu phải trước ngày kết thúc"));
        }

        var result = await _reportService.GetRevenueByProductAsync(fromDate, toDate, limit, cancellationToken);
        return Ok(ApiResponse<List<RevenueByProductDto>>.Success("Lấy dữ liệu doanh thu theo sản phẩm thành công", result));
    }
}