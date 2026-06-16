using Microsoft.EntityFrameworkCore;
using StoreManagement.Common;
using StoreManagement.Data;
using StoreManagement.Extensions;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;

    public ProductRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Products?> GetByIdAsync(int id, bool showDeleted = false, CancellationToken cancellationToken = default)
    {
        return BaseQuery(showDeleted)
            .Include(x => x.ProductImages)
            .FirstOrDefaultAsync(x => x.IdProduct == id, cancellationToken);
    }

    public Task<Products?> GetByProductCodeAsync(string code, bool showDeleted = false, CancellationToken cancellationToken = default)
    {
        return BaseQuery(showDeleted)
            .FirstOrDefaultAsync(x => x.ProductCode == code, cancellationToken);
    }

    public Task<Products?> GetBySkuAsync(string sku, bool showDeleted = false, CancellationToken cancellationToken = default)
    {
        return BaseQuery(showDeleted)
            .FirstOrDefaultAsync(x => x.Sku == sku, cancellationToken);
    }

    public Task<PagedResult<Products>> SearchAsync(string? keyword, int? categoryId, int? supplierId, string? brand, decimal? minPrice, decimal? maxPrice, string? inventoryStatus, bool showDeleted, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var query = BaseQuery(showDeleted).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalized = keyword.Trim().ToLower();
            query = query.Where(x =>
                x.ProductCode.ToLower().Contains(normalized) ||
                x.ProductName.ToLower().Contains(normalized) ||
                (x.Sku != null && x.Sku.ToLower().Contains(normalized)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.IdCategory == categoryId.Value);
        }

        if (supplierId.HasValue)
        {
            query = query.Where(x => x.IdSupplier == supplierId.Value);
        }

        if (!string.IsNullOrWhiteSpace(brand))
        {
            var normalizedBrand = brand.Trim().ToLower();
            query = query.Where(x => x.Brand != null && x.Brand.ToLower().Contains(normalizedBrand));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(x => x.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= maxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(inventoryStatus))
        {
            query = inventoryStatus.Trim().ToUpperInvariant() switch
            {
                "COMING_SOON" => query.Where(x => (x.StockQuantity == null || x.StockQuantity == 0) && x.Status != "DISCONTINUED"),
                "IN_STOCK" => query.Where(x => (x.StockQuantity ?? 0) >= 10),
                "LOW_STOCK" => query.Where(x => (x.StockQuantity ?? 0) > 0 && (x.StockQuantity ?? 0) < 10),
                "OUT_OF_STOCK" => query.Where(x => (x.StockQuantity == null || x.StockQuantity == 0) && x.Status == "OUT_OF_STOCK"),
                _ => throw new InvalidOperationException("Trạng thái tồn kho không hợp lệ")
            };
        }

        return query.ApplySorting(sortBy, sortDirection).ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public Task<List<Products>> GetByIdsAsync(List<int> ids, bool showDeleted = false, CancellationToken cancellationToken = default)
    {
        return BaseQuery(showDeleted)
            .Where(x => ids.Contains(x.IdProduct))
            .ToListAsync(cancellationToken);
    }

    public Task<List<string>> GetDistinctBrandsAsync(bool showDeleted, CancellationToken cancellationToken = default)
    {
        return BaseQuery(showDeleted)
            .Where(x => x.Brand != null && x.Brand != string.Empty)
            .Select(x => x.Brand!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Products>> GetRelatedProductsAsync(int categoryId, int productId, int limit, bool showDeleted, CancellationToken cancellationToken = default)
    {
        var query = BaseQuery(showDeleted)
            .Where(x => x.IdCategory == categoryId && x.IdProduct != productId);

        if (!showDeleted)
        {
            query = query.Where(x => x.Status == "IN_STOCK");
        }

        return query.OrderByDescending(x => x.CreatedAt).Take(limit).ToListAsync(cancellationToken);
    }

    public Task<List<Products>> GetNewProductsAsync(int pageNo, int pageSize, bool showDeleted, CancellationToken cancellationToken = default)
    {
        var query = BaseQuery(showDeleted);
        if (!showDeleted)
        {
            query = query.Where(x => x.Status == "IN_STOCK");
        }

        return query.OrderByDescending(x => x.CreatedAt).Skip((pageNo - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Products product, CancellationToken cancellationToken = default)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Products> BaseQuery(bool showDeleted)
    {
        var query = _dbContext.Products
            .Include(x => x.IdCategoryNavigation)
            .Include(x => x.IdSupplierNavigation)
            .AsQueryable();

        if (!showDeleted)
        {
            query = query.Where(x => !x.IsDelete);
        }

        return query;
    }
}
