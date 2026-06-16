using StoreManagement.Common;
using StoreManagement.Dtos.Category;

namespace StoreManagement.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<PageResponse<CategoryDto>> GetAllCategoriesPaginatedAsync(int pageNo, int pageSize, string sortBy, string sortDirection, string? name, CancellationToken cancellationToken = default);
    Task<CategoryDto> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateCategoryAsync(int id, CategoryDto categoryDto, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
}
