using StoreManagement.Common;
using StoreManagement.Dtos.Category;
using StoreManagement.Exceptions;
using StoreManagement.Extensions;
using StoreManagement.Models;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        return categories.Select(MapToDto).ToList();
    }

    public async Task<PageResponse<CategoryDto>> GetAllCategoriesPaginatedAsync(int pageNo, int pageSize, string sortBy, string sortDirection, string? name, CancellationToken cancellationToken = default)
    {
        var page = await _categoryRepository.GetPagedAsync(pageNo, pageSize, sortBy, sortDirection, name, cancellationToken);
        return new PagedResult<CategoryDto>
        {
            Items = page.Items.Select(MapToDto).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Danh mục không tồn tại với ID: {id}");
        return MapToDto(category);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto, CancellationToken cancellationToken = default)
    {
        if (await _categoryRepository.GetByNameAsync(categoryDto.CategoryName, cancellationToken) is not null)
        {
            throw new ConflictException($"Tên danh mục đã tồn tại: {categoryDto.CategoryName}");
        }

        var category = new Categories
        {
            CategoryName = categoryDto.CategoryName,
            CodePrefix = categoryDto.CodePrefix
        };

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(category);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(int id, CategoryDto categoryDto, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Danh mục không tồn tại với ID: {id}");

        if (!string.Equals(category.CategoryName, categoryDto.CategoryName, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _categoryRepository.GetByNameAsync(categoryDto.CategoryName, cancellationToken);
            if (existing is not null && existing.IdCategory != id)
            {
                throw new ConflictException($"Tên danh mục đã tồn tại: {categoryDto.CategoryName}");
            }

            category.CategoryName = categoryDto.CategoryName;
        }

        category.CodePrefix = categoryDto.CodePrefix;
        await _categoryRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(category);
    }

    public async Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Danh mục không tồn tại với ID: {id}");
        await _categoryRepository.DeleteAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
    }

    private static CategoryDto MapToDto(Categories category)
    {
        return new CategoryDto
        {
            IdCategory = category.IdCategory,
            CategoryName = category.CategoryName,
            CodePrefix = category.CodePrefix,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}
