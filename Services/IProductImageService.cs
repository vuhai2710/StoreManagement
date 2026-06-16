using StoreManagement.Dtos.Product;

namespace StoreManagement.Services;

public interface IProductImageService
{
    Task<List<ProductImageDto>> UploadProductImagesAsync(int productId, List<IFormFile> images, CancellationToken cancellationToken = default);
    Task<ProductImageDto> AddProductImageAsync(int productId, IFormFile image, CancellationToken cancellationToken = default);
    Task<List<ProductImageDto>> GetProductImagesAsync(int productId, CancellationToken cancellationToken = default);
    Task DeleteProductImageAsync(int imageId, CancellationToken cancellationToken = default);
    Task<ProductImageDto> SetImageAsPrimaryAsync(int imageId, CancellationToken cancellationToken = default);
}
