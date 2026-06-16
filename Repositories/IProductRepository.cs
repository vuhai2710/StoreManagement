using StoreManagement.Common;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface IProductRepository
{
    Task<Products?> GetByIdAsync(int id, bool showDeleted = false, CancellationToken cancellationToken = default);
    Task<Products?> GetByProductCodeAsync(string code, bool showDeleted = false, CancellationToken cancellationToken = default);
    Task<Products?> GetBySkuAsync(string sku, bool showDeleted = false, CancellationToken cancellationToken = default);
    Task<PagedResult<Products>> SearchAsync(string? keyword, int? categoryId, int? supplierId, string? brand, decimal? minPrice, decimal? maxPrice, string? inventoryStatus, bool showDeleted, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<List<Products>> GetByIdsAsync(List<int> ids, bool showDeleted = false, CancellationToken cancellationToken = default);
    Task<List<string>> GetDistinctBrandsAsync(bool showDeleted, CancellationToken cancellationToken = default);
    Task<List<Products>> GetRelatedProductsAsync(int categoryId, int productId, int limit, bool showDeleted, CancellationToken cancellationToken = default);
    Task<List<Products>> GetNewProductsAsync(int pageNo, int pageSize, bool showDeleted, CancellationToken cancellationToken = default);
    Task AddAsync(Products product, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
