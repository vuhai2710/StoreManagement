using StoreManagement.Common;
using StoreManagement.Dtos.Product;
using StoreManagement.Exceptions;
using StoreManagement.Extensions;
using StoreManagement.Models;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IProductReviewRepository _productReviewRepository;
    private readonly IOrderRepository _orderRepository;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ISupplierRepository supplierRepository,
        IProductReviewRepository productReviewRepository,
        IOrderRepository orderRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _supplierRepository = supplierRepository;
        _productReviewRepository = productReviewRepository;
        _orderRepository = orderRepository;
    }

    public async Task<PageResponse<ProductDto>> GetAllProductsAsync(int pageNo, int pageSize, string sortBy, string sortDirection, string? keyword, int? categoryId, string? brand, decimal? minPrice, decimal? maxPrice, string? inventoryStatus, bool showDeleted, CancellationToken cancellationToken = default)
    {
        var page = await _productRepository.SearchAsync(keyword, categoryId, null, brand, minPrice, maxPrice, inventoryStatus, showDeleted, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return await MapPageAsync(page, cancellationToken);
    }

    public async Task<ProductDto> GetProductByIdAsync(int id, bool showDeleted, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, showDeleted, cancellationToken)
            ?? throw new ResourceNotFoundException($"Sản phẩm không tồn tại with ID: {id}");
        return await MapToDtoAsync(product, cancellationToken);
    }

    public async Task<ProductDto> GetProductByCodeAsync(string code, bool showDeleted, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByProductCodeAsync(code, showDeleted, cancellationToken)
            ?? throw new ResourceNotFoundException($"Sản phẩm không tồn tại with mã: {code}");
        return await MapToDtoAsync(product, cancellationToken);
    }

    public Task<PageResponse<ProductDto>> SearchProductsByNameAsync(string name, int pageNo, int pageSize, string sortBy, string sortDirection, bool showDeleted, CancellationToken cancellationToken = default)
    {
        return GetAllProductsAsync(pageNo, pageSize, sortBy, sortDirection, name, null, null, null, null, null, showDeleted, cancellationToken);
    }

    public async Task<PageResponse<ProductDto>> GetProductsByCategoryAsync(int categoryId, int pageNo, int pageSize, string sortBy, string sortDirection, bool showDeleted, CancellationToken cancellationToken = default)
    {
        _ = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Danh mục không tồn tại với ID: {categoryId}");
        return await GetAllProductsAsync(pageNo, pageSize, sortBy, sortDirection, null, categoryId, null, null, null, null, showDeleted, cancellationToken);
    }

    public Task<PageResponse<ProductDto>> GetProductsByBrandAsync(string brand, int pageNo, int pageSize, string sortBy, string sortDirection, bool showDeleted, CancellationToken cancellationToken = default)
    {
        return GetAllProductsAsync(pageNo, pageSize, sortBy, sortDirection, null, null, brand, null, null, null, showDeleted, cancellationToken);
    }

    public async Task<PageResponse<ProductDto>> GetProductsBySupplierAsync(int supplierId, int pageNo, int pageSize, string sortBy, string sortDirection, bool showDeleted, CancellationToken cancellationToken = default)
    {
        _ = await _supplierRepository.GetByIdAsync(supplierId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Nhà cung cấp không tồn tại với ID: {supplierId}");
        var page = await _productRepository.SearchAsync(null, null, supplierId, null, null, null, null, showDeleted, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return await MapPageAsync(page, cancellationToken);
    }

    public Task<PageResponse<ProductDto>> GetProductsByPriceRangeAsync(decimal? minPrice, decimal? maxPrice, int pageNo, int pageSize, string sortBy, string sortDirection, bool showDeleted, CancellationToken cancellationToken = default)
    {
        return GetAllProductsAsync(pageNo, pageSize, sortBy, sortDirection, null, null, null, minPrice, maxPrice, null, showDeleted, cancellationToken);
    }

    public async Task<PageResponse<ProductDto>> GetBestSellingProductsAsync(string? status, int pageNo, int pageSize, bool showDeleted, CancellationToken cancellationToken = default)
    {
        var pageNoZeroBased = pageNo - 1;
        var ids = await _orderRepository.GetBestSellingProductIdsAsync(string.IsNullOrWhiteSpace(status) ? null : status.Trim(), pageSize, pageNoZeroBased * pageSize, cancellationToken);
        var productIds = ids.Select(x => x.ProductId).ToList();
        var products = await _productRepository.GetByIdsAsync(productIds, showDeleted, cancellationToken);
        var items = new List<ProductDto>();
        foreach (var productId in productIds)
        {
            var product = products.FirstOrDefault(x => x.IdProduct == productId);
            if (product is not null)
            {
                items.Add(await MapToDtoAsync(product, cancellationToken));
            }
        }

        var totalElements = await _orderRepository.CountBestSellingProductsAsync(string.IsNullOrWhiteSpace(status) ? null : status.Trim(), cancellationToken);
        var totalPages = totalElements == 0 ? 0 : (int)Math.Ceiling(totalElements / (double)pageSize);
        return new PageResponse<ProductDto>
        {
            Content = items,
            PageNo = pageNo,
            PageSize = pageSize,
            TotalElements = totalElements,
            TotalPages = totalPages,
            IsFirst = pageNo <= 1,
            IsLast = totalPages == 0 || pageNo >= totalPages,
            HasNext = totalPages > 0 && pageNo < totalPages,
            HasPrevious = pageNo > 1,
            IsEmpty = items.Count == 0
        };
    }

    public async Task<List<ProductDto>> GetTop5BestSellingProductsAsync(string? status, bool showDeleted, CancellationToken cancellationToken = default)
    {
        var page = await GetBestSellingProductsAsync(status, 1, 5, showDeleted, cancellationToken);
        return page.Content;
    }

    public async Task<PageResponse<ProductDto>> GetNewProductsAsync(int pageNo, int pageSize, int? limit, bool showDeleted, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetNewProductsAsync(pageNo, pageSize, showDeleted, cancellationToken);
        var mapped = new List<ProductDto>();
        foreach (var product in products)
        {
            mapped.Add(await MapToDtoAsync(product, cancellationToken));
        }

        if (limit.HasValue && limit.Value > 0 && mapped.Count > limit.Value)
        {
            mapped = mapped.Take(limit.Value).ToList();
        }

        return new PageResponse<ProductDto>
        {
            Content = mapped,
            PageNo = pageNo,
            PageSize = pageSize,
            TotalElements = mapped.Count,
            TotalPages = 1,
            IsFirst = true,
            IsLast = true,
            HasNext = false,
            HasPrevious = false,
            IsEmpty = mapped.Count == 0
        };
    }

    public async Task<List<ProductDto>> GetRelatedProductsAsync(int id, int limit, bool showDeleted, CancellationToken cancellationToken = default)
    {
        var current = await _productRepository.GetByIdAsync(id, showDeleted, cancellationToken)
            ?? throw new ResourceNotFoundException("Sản phẩm không tồn tại");
        if (!current.IdCategory.HasValue)
        {
            return [];
        }

        var related = await _productRepository.GetRelatedProductsAsync(current.IdCategory.Value, id, limit, showDeleted, cancellationToken);
        var result = new List<ProductDto>();
        foreach (var item in related)
        {
            result.Add(await MapToDtoAsync(item, cancellationToken));
        }

        return result;
    }

    public Task<List<string>> GetAllBrandsAsync(bool showDeleted, CancellationToken cancellationToken = default)
    {
        return _productRepository.GetDistinctBrandsAsync(showDeleted, cancellationToken);
    }

    public async Task<ProductDto> CreateProductAsync(ProductDto productDto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productDto.CodeType))
        {
            throw new InvalidOperationException("Loại mã không hợp lệ");
        }

        var category = await _categoryRepository.GetByIdAsync(productDto.IdCategory ?? 0, cancellationToken)
            ?? throw new ResourceNotFoundException($"Danh mục không tồn tại với ID: {productDto.IdCategory}");

        if (!string.IsNullOrWhiteSpace(productDto.ProductCode))
        {
            if (await _productRepository.GetByProductCodeAsync(productDto.ProductCode, true, cancellationToken) is not null)
            {
                throw new ConflictException($"Mã sản phẩm đã tồn tại: {productDto.ProductCode}");
            }
        }

        if (!string.IsNullOrWhiteSpace(productDto.Sku))
        {
            if (await _productRepository.GetBySkuAsync(productDto.Sku, true, cancellationToken) is not null)
            {
                throw new ConflictException($"SKU đã tồn tại: {productDto.Sku}");
            }
        }

        Suppliers? supplier = null;
        if (productDto.IdSupplier.HasValue)
        {
            supplier = await _supplierRepository.GetByIdAsync(productDto.IdSupplier.Value, cancellationToken)
                ?? throw new ResourceNotFoundException($"Nhà cung cấp không tồn tại với ID: {productDto.IdSupplier}");
        }

        var entity = new Products
        {
            IdCategory = category.IdCategory,
            IdSupplier = supplier?.IdSupplier,
            ProductName = productDto.ProductName,
            Brand = productDto.Brand,
            Description = productDto.Description,
            Price = productDto.Price ?? 0,
            StockQuantity = productDto.StockQuantity ?? 0,
            Status = productDto.Status ?? ((productDto.StockQuantity ?? 0) > 0 ? "IN_STOCK" : "OUT_OF_STOCK"),
            ImageUrl = productDto.ImageUrl,
            ProductCode = productDto.ProductCode ?? Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
            CodeType = productDto.CodeType,
            Sku = productDto.Sku,
            IsDelete = false
        };

        await _productRepository.AddAsync(entity, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);
        return await MapToDtoAsync(entity, cancellationToken);
    }

    public async Task<ProductDto> UpdateProductAsync(int id, ProductDto productDto, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, true, cancellationToken)
            ?? throw new ResourceNotFoundException($"Sản phẩm không tồn tại với ID: {id}");

        if (!string.IsNullOrWhiteSpace(productDto.ProductCode) && !string.Equals(productDto.ProductCode, product.ProductCode, StringComparison.OrdinalIgnoreCase))
        {
            if (await _productRepository.GetByProductCodeAsync(productDto.ProductCode, true, cancellationToken) is not null)
            {
                throw new ConflictException($"Mã sản phẩm đã tồn tại: {productDto.ProductCode}");
            }

            product.ProductCode = productDto.ProductCode;
        }

        if (!string.IsNullOrWhiteSpace(productDto.Sku) && !string.Equals(productDto.Sku, product.Sku, StringComparison.OrdinalIgnoreCase))
        {
            if (await _productRepository.GetBySkuAsync(productDto.Sku, true, cancellationToken) is not null)
            {
                throw new ConflictException($"SKU đã tồn tại: {productDto.Sku}");
            }

            product.Sku = productDto.Sku;
        }

        if (productDto.IdCategory.HasValue && productDto.IdCategory != product.IdCategory)
        {
            _ = await _categoryRepository.GetByIdAsync(productDto.IdCategory.Value, cancellationToken)
                ?? throw new ResourceNotFoundException($"Danh mục không tồn tại với ID: {productDto.IdCategory}");
            product.IdCategory = productDto.IdCategory;
        }

        if (productDto.IdSupplier.HasValue)
        {
            _ = await _supplierRepository.GetByIdAsync(productDto.IdSupplier.Value, cancellationToken)
                ?? throw new ResourceNotFoundException($"Nhà cung cấp không tồn tại với ID: {productDto.IdSupplier}");
            product.IdSupplier = productDto.IdSupplier;
        }

        product.ProductName = productDto.ProductName ?? product.ProductName;
        product.Brand = productDto.Brand ?? product.Brand;
        product.Description = productDto.Description ?? product.Description;
        product.Price = productDto.Price ?? product.Price;
        product.StockQuantity = productDto.StockQuantity ?? product.StockQuantity;
        product.Status = productDto.Status ?? product.Status;
        product.ImageUrl = productDto.ImageUrl ?? product.ImageUrl;
        product.CodeType = productDto.CodeType ?? product.CodeType;

        await _productRepository.SaveChangesAsync(cancellationToken);
        return await MapToDtoAsync(product, cancellationToken);
    }

    public async Task DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, true, cancellationToken)
            ?? throw new ResourceNotFoundException($"Sản phẩm không tồn tại với ID: {id}");
        product.IsDelete = true;
        await _productRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, true, cancellationToken)
            ?? throw new ResourceNotFoundException($"Sản phẩm không tồn tại với ID: {id}");
        product.IsDelete = false;
        await _productRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<PageResponse<ProductDto>> MapPageAsync(PagedResult<Products> page, CancellationToken cancellationToken)
    {
        var items = new List<ProductDto>();
        foreach (var product in page.Items)
        {
            items.Add(await MapToDtoAsync(product, cancellationToken));
        }

        return new PagedResult<ProductDto>
        {
            Items = items,
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    private async Task<ProductDto> MapToDtoAsync(Products product, CancellationToken cancellationToken)
    {
        var summary = await _productReviewRepository.GetSummaryAsync(product.IdProduct, cancellationToken);
        return new ProductDto
        {
            IdProduct = product.IdProduct,
            IdCategory = product.IdCategory,
            CategoryName = product.IdCategoryNavigation?.CategoryName,
            ProductName = product.ProductName,
            Brand = product.Brand,
            IdSupplier = product.IdSupplier,
            SupplierName = product.IdSupplierNavigation?.SupplierName,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Status = product.Status,
            ImageUrl = product.ImageUrl,
            Images = product.ProductImages?.Select(image => new ProductImageDto
            {
                IdProductImage = image.IdProductImage,
                IdProduct = image.IdProduct,
                ImageUrl = image.ImageUrl,
                IsPrimary = image.IsPrimary,
                DisplayOrder = image.DisplayOrder,
                CreatedAt = image.CreatedAt
            }).ToList(),
            ProductCode = product.ProductCode,
            CodeType = product.CodeType ?? string.Empty,
            Sku = product.Sku,
            AverageRating = summary.AverageRating,
            ReviewCount = summary.ReviewCount,
            IsDelete = product.IsDelete,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
