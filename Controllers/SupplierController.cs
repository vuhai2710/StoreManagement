using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Supplier;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/suppliers")]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SupplierController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet("all")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<List<SupplierDto>>>> GetAllSuppliers(CancellationToken cancellationToken)
    {
        var suppliers = await _supplierService.GetAllSuppliersAsync(cancellationToken);
        return Ok(ApiResponse<List<SupplierDto>>.Success("Lấy danh sách nhà cung cấp thành công", suppliers));
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<SupplierDto>>>> GetAllSuppliersPaginated(
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "IdSupplier",
        [FromQuery] string sortDirection = "ASC",
        [FromQuery] string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        var page = await _supplierService.GetAllSuppliersPaginatedAsync(pageNo, pageSize, sortBy, sortDirection, keyword, cancellationToken);
        return Ok(ApiResponse<PageResponse<SupplierDto>>.Success("Lấy danh sách nhà cung cấp thành công", page));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> GetSupplierById(int id, CancellationToken cancellationToken)
    {
        var supplier = await _supplierService.GetSupplierByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<SupplierDto>.Success("Lấy thông tin nhà cung cấp thành công", supplier));
    }

    [HttpGet("search")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<SupplierDto>>>> SearchSuppliersByName(
        [FromQuery] string name,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "IdSupplier",
        [FromQuery] string sortDirection = "ASC",
        CancellationToken cancellationToken = default)
    {
        var page = await _supplierService.GetAllSuppliersPaginatedAsync(pageNo, pageSize, sortBy, sortDirection, name, cancellationToken);
        return Ok(ApiResponse<PageResponse<SupplierDto>>.Success("Tìm kiếm nhà cung cấp thành công", page));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> CreateSupplier([FromBody] SupplierDto supplierDto, CancellationToken cancellationToken)
    {
        var supplier = await _supplierService.CreateSupplierAsync(supplierDto, cancellationToken);
        return Ok(ApiResponse<SupplierDto>.Success("Thêm nhà cung cấp thành công", supplier));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> UpdateSupplier(int id, [FromBody] SupplierDto supplierDto, CancellationToken cancellationToken)
    {
        var updated = await _supplierService.UpdateSupplierAsync(id, supplierDto, cancellationToken);
        return Ok(ApiResponse<SupplierDto>.Success("Cập nhật nhà cung cấp thành công", updated));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSupplier(int id, CancellationToken cancellationToken)
    {
        await _supplierService.DeleteSupplierAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Success("Xóa nhà cung cấp thành công", null));
    }
}
