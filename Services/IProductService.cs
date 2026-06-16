using StoreManagement.Common;
using StoreManagement.Dtos.Product;

namespace StoreManagement.Services;

public interface IProductService
{
    Task<PageResponse<ProductDto>> GetAllProductsAsync(int pageNo, int pageSize, string sortBy, string sortDirection, string? keyword, int? categoryId, string? brand, decimal? minPrice, decimal? maxPrice, string? inventoryStatus, bool showDeleted, CancellationToken cancellationToken = default);
    Task<ProductDto> GetProductByIdAsync(int id, bool showDeleted, CancellationToken cancellationToken = default);
    Task<ProductDto> GetProductByCodeAsync(string code, bool showDeleted, CancellationToken cancellationToken = default);
    Task<PageResponse<ProductDto>> SearchProductsByNameAsync(string name, int pageNo, int pageSize, string sortBy, string sortDirection, bool showDeleted, CancellationToken cancellationToken = default);
    Task<PageResponse<ProductDto>> GetProductsByCategoryAsync(int categoryId, int pageNo, int pageSize, string sortBy, string sortDirection, bool showDeleted, CancellationToken cancellationToken = default);
    Task<PageResponse<ProductDto>> GetProductsByBrandAsync(string brand, int pageNo, int pageSize, string sortBy, string sortDirection, bool showDeleted, CancellationToken cancellationToken = default);
    Task<PageResponse<ProductDto>> GetProductsBySupplierAsync(int supplierId, int pageNo, int pageSize, string sortBy, string sortDirection, bool showDeleted, CancellationToken cancellationToken = default);
    Task<PageResponse<ProductDto>> GetProductsByPriceRangeAsync(decimal? minPrice, decimal? maxPrice, int pageNo, int pageSize, string sortBy, string sortDirection, bool showDeleted, CancellationToken cancellationToken = default);
    Task<PageResponse<ProductDto>> GetBestSellingProductsAsync(string? status, int pageNo, int pageSize, bool showDeleted, CancellationToken cancellationToken = default);
    Task<List<ProductDto>> GetTop5BestSellingProductsAsync(string? status, bool showDeleted, CancellationToken cancellationToken = default);
    Task<PageResponse<ProductDto>> GetNewProductsAsync(int pageNo, int pageSize, int? limit, bool showDeleted, CancellationToken cancellationToken = default);
    Task<List<ProductDto>> GetRelatedProductsAsync(int id, int limit, bool showDeleted, CancellationToken cancellationToken = default);
    Task<List<string>> GetAllBrandsAsync(bool showDeleted, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateProductAsync(ProductDto productDto, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateProductAsync(int id, ProductDto productDto, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(int id, CancellationToken cancellationToken = default);
    Task RestoreProductAsync(int id, CancellationToken cancellationToken = default);
}
