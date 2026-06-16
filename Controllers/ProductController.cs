using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Product;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IProductImageService _productImageService;
    private readonly IProductViewService _productViewService;
    private readonly IPromotionService _promotionService;

    public ProductController(IProductService productService, IProductImageService productImageService, IProductViewService productViewService, IPromotionService promotionService)
    {
        _productService = productService;
        _productImageService = productImageService;
        _productViewService = productViewService;
        _promotionService = promotionService;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<PageResponse<ProductDto>>>> GetAllProducts(
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "IdProduct",
        [FromQuery] string sortDirection = "ASC",
        [FromQuery] string? keyword = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? brand = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? inventoryStatus = null,
        [FromQuery] bool showDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var page = await _productService.GetAllProductsAsync(pageNo, pageSize, sortBy, sortDirection, keyword, categoryId, brand, minPrice, maxPrice, inventoryStatus, showDeleted, cancellationToken);
        return Ok(ApiResponse<PageResponse<ProductDto>>.Success("Lấy danh sách sản phẩm thành công", page));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProductById(int id, [FromQuery] bool showDeleted = false, CancellationToken cancellationToken = default)
    {
        var product = await _productService.GetProductByIdAsync(id, showDeleted, cancellationToken);
        int? userId = null;
        if (int.TryParse(User.Claims.FirstOrDefault(x => x.Type == "idUser")?.Value, out var parsedUserId))
        {
            userId = parsedUserId;
        }
        await _productViewService.LogViewAsync(userId, HttpContext.Session.Id, id, cancellationToken);
        return Ok(ApiResponse<ProductDto>.Success("Lấy thông tin sản phẩm thành công", product));
    }

    [HttpGet("code/{code}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProductByCode(string code, [FromQuery] bool showDeleted = false, CancellationToken cancellationToken = default)
    {
        var product = await _productService.GetProductByCodeAsync(code, showDeleted, cancellationToken);
        return Ok(ApiResponse<ProductDto>.Success("Tìm sản phẩm theo mã thành công", product));
    }

    [HttpGet("search/name")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<PageResponse<ProductDto>>>> SearchProductsByName(
        [FromQuery] string name,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "IdProduct",
        [FromQuery] string sortDirection = "ASC",
        [FromQuery] bool showDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var page = await _productService.SearchProductsByNameAsync(name, pageNo, pageSize, sortBy, sortDirection, showDeleted, cancellationToken);
        return Ok(ApiResponse<PageResponse<ProductDto>>.Success("Tìm kiếm sản phẩm theo tên thành công", page));
    }

    [HttpGet("category/{categoryId:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<PageResponse<ProductDto>>>> GetProductsByCategory(int categoryId, [FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "IdProduct", [FromQuery] string sortDirection = "ASC", [FromQuery] bool showDeleted = false, CancellationToken cancellationToken = default)
    {
        var page = await _productService.GetProductsByCategoryAsync(categoryId, pageNo, pageSize, sortBy, sortDirection, showDeleted, cancellationToken);
        return Ok(ApiResponse<PageResponse<ProductDto>>.Success("Lấy danh sách sản phẩm theo danh mục thành công", page));
    }

    [HttpGet("brand/{brand}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<PageResponse<ProductDto>>>> GetProductsByBrand(string brand, [FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "IdProduct", [FromQuery] string sortDirection = "ASC", [FromQuery] bool showDeleted = false, CancellationToken cancellationToken = default)
    {
        var page = await _productService.GetProductsByBrandAsync(brand, pageNo, pageSize, sortBy, sortDirection, showDeleted, cancellationToken);
        return Ok(ApiResponse<PageResponse<ProductDto>>.Success("Lấy danh sách sản phẩm theo thương hiệu thành công", page));
    }

    [HttpGet("supplier/{supplierId:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<ProductDto>>>> GetProductsBySupplier(int supplierId, [FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "IdProduct", [FromQuery] string sortDirection = "ASC", [FromQuery] bool showDeleted = false, CancellationToken cancellationToken = default)
    {
        var page = await _productService.GetProductsBySupplierAsync(supplierId, pageNo, pageSize, sortBy, sortDirection, showDeleted, cancellationToken);
        return Ok(ApiResponse<PageResponse<ProductDto>>.Success("Lấy danh sách sản phẩm theo nhà cung cấp thành công", page));
    }

    [HttpGet("price")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<PageResponse<ProductDto>>>> GetProductsByPrice([FromQuery] decimal? minPrice = null, [FromQuery] decimal? maxPrice = null, [FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "Price", [FromQuery] string sortDirection = "ASC", [FromQuery] bool showDeleted = false, CancellationToken cancellationToken = default)
    {
        var page = await _productService.GetProductsByPriceRangeAsync(minPrice, maxPrice, pageNo, pageSize, sortBy, sortDirection, showDeleted, cancellationToken);
        return Ok(ApiResponse<PageResponse<ProductDto>>.Success("Lấy danh sách sản phẩm theo khoảng giá thành công", page));
    }

    [HttpGet("best-sellers")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<PageResponse<ProductDto>>>> GetBestSellingProducts([FromQuery] string? status = null, [FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] bool showDeleted = false, CancellationToken cancellationToken = default)
    {
        var page = await _productService.GetBestSellingProductsAsync(status, pageNo, pageSize, showDeleted, cancellationToken);
        return Ok(ApiResponse<PageResponse<ProductDto>>.Success("Lấy danh sách sản phẩm bán chạy thành công", page));
    }

    [HttpGet("best-sellers/top-5")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetTop5BestSellingProducts([FromQuery] string? status = null, [FromQuery] bool showDeleted = false, CancellationToken cancellationToken = default)
    {
        var products = await _productService.GetTop5BestSellingProductsAsync(status, showDeleted, cancellationToken);
        return Ok(ApiResponse<List<ProductDto>>.Success("Lấy top 5 sản phẩm bán chạy thành công", products));
    }

    [HttpGet("new")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<PageResponse<ProductDto>>>> GetNewProducts([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] int? limit = null, [FromQuery] bool showDeleted = false, CancellationToken cancellationToken = default)
    {
        var page = await _productService.GetNewProductsAsync(pageNo, pageSize, limit, showDeleted, cancellationToken);
        return Ok(ApiResponse<PageResponse<ProductDto>>.Success("Lấy danh sách sản phẩm mới thành công", page));
    }

    [HttpGet("on-sale")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<List<ProductOnSaleDto>>>> GetProductsOnSale(CancellationToken cancellationToken)
    {
        var items = await _promotionService.GetProductsOnSaleAsync(cancellationToken);
        return Ok(ApiResponse<List<ProductOnSaleDto>>.Success("Lấy danh sách sản phẩm đang giảm giá thành công", items));
    }

    [HttpGet("{id:int}/related")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetRelatedProducts(int id, [FromQuery] int limit = 8, [FromQuery] bool showDeleted = false, CancellationToken cancellationToken = default)
    {
        var items = await _productService.GetRelatedProductsAsync(id, limit, showDeleted, cancellationToken);
        return Ok(ApiResponse<List<ProductDto>>.Success("Lấy danh sách sản phẩm liên quan thành công", items));
    }

    [HttpGet("brands")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetAllBrands([FromQuery] bool showDeleted = false, CancellationToken cancellationToken = default)
    {
        var brands = await _productService.GetAllBrandsAsync(showDeleted, cancellationToken);
        return Ok(ApiResponse<List<string>>.Success("Lấy danh sách thương hiệu thành công", brands));
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] ProductDto productDto, CancellationToken cancellationToken)
    {
        var product = await _productService.CreateProductAsync(productDto, cancellationToken);
        return Ok(ApiResponse<ProductDto>.Success("Thêm sản phẩm thành công", product));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(int id, [FromBody] ProductDto productDto, CancellationToken cancellationToken)
    {
        var product = await _productService.UpdateProductAsync(id, productDto, cancellationToken);
        return Ok(ApiResponse<ProductDto>.Success("Cập nhật sản phẩm thành công", product));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        await _productService.DeleteProductAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Success("Xóa sản phẩm thành công", null));
    }

    [HttpPut("{id:int}/restore")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<object>>> RestoreProduct(int id, CancellationToken cancellationToken)
    {
        await _productService.RestoreProductAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Success("Khôi phục sản phẩm thành công", null));
    }

    [HttpPost("{id:int}/images")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<List<ProductImageDto>>>> UploadProductImages(int id, [FromForm] List<IFormFile> images, CancellationToken cancellationToken)
    {
        var result = await _productImageService.UploadProductImagesAsync(id, images, cancellationToken);
        return Ok(ApiResponse<List<ProductImageDto>>.Success("Upload ảnh thành công", result));
    }

    [HttpPost("{id:int}/images/single")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<ProductImageDto>>> AddProductImage(int id, [FromForm] IFormFile image, CancellationToken cancellationToken)
    {
        var result = await _productImageService.AddProductImageAsync(id, image, cancellationToken);
        return Ok(ApiResponse<ProductImageDto>.Success("Thêm ảnh thành công", result));
    }

    [HttpGet("{id:int}/images")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<List<ProductImageDto>>>> GetProductImages(int id, CancellationToken cancellationToken)
    {
        var images = await _productImageService.GetProductImagesAsync(id, cancellationToken);
        return Ok(ApiResponse<List<ProductImageDto>>.Success("Lấy danh sách ảnh thành công", images));
    }

    [HttpDelete("images/{imageId:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProductImage(int imageId, CancellationToken cancellationToken)
    {
        await _productImageService.DeleteProductImageAsync(imageId, cancellationToken);
        return Ok(ApiResponse<object>.Success("Xóa ảnh thành công", null));
    }

    [HttpPut("images/{imageId:int}/primary")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<ProductImageDto>>> SetImageAsPrimary(int imageId, CancellationToken cancellationToken)
    {
        var image = await _productImageService.SetImageAsPrimaryAsync(imageId, cancellationToken);
        return Ok(ApiResponse<ProductImageDto>.Success("Đặt ảnh chính thành công", image));
    }
}
