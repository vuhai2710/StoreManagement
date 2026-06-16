using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Category;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("all")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAllCategories(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);
        return Ok(ApiResponse<List<CategoryDto>>.Success("Lấy danh sách danh mục thành công", categories));
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<CategoryDto>>>> GetAllCategoriesPaginated(
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "IdCategory",
        [FromQuery] string sortDirection = "ASC",
        [FromQuery] string? name = null,
        CancellationToken cancellationToken = default)
    {
        var page = await _categoryService.GetAllCategoriesPaginatedAsync(pageNo, pageSize, sortBy, sortDirection, name, cancellationToken);
        return Ok(ApiResponse<PageResponse<CategoryDto>>.Success("Lấy danh sách danh mục thành công", page));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategoryById(int id, CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<CategoryDto>.Success("Lấy thông tin danh mục thành công", category));
    }

    [HttpGet("search")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<CategoryDto>>>> SearchCategoriesByName(
        [FromQuery] string name,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "IdCategory",
        [FromQuery] string sortDirection = "ASC",
        CancellationToken cancellationToken = default)
    {
        var page = await _categoryService.GetAllCategoriesPaginatedAsync(pageNo, pageSize, sortBy, sortDirection, name, cancellationToken);
        return Ok(ApiResponse<PageResponse<CategoryDto>>.Success("Tìm kiếm danh mục thành công", page));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CategoryDto categoryDto, CancellationToken cancellationToken)
    {
        var createdCategory = await _categoryService.CreateCategoryAsync(categoryDto, cancellationToken);
        return Ok(ApiResponse<CategoryDto>.Success("Thêm danh mục thành công", createdCategory));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(int id, [FromBody] CategoryDto categoryDto, CancellationToken cancellationToken)
    {
        var updatedCategory = await _categoryService.UpdateCategoryAsync(id, categoryDto, cancellationToken);
        return Ok(ApiResponse<CategoryDto>.Success("Cập nhật danh mục thành công", updatedCategory));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        await _categoryService.DeleteCategoryAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Success("Xóa danh mục thành công", null));
    }
}
