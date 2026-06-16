using StoreManagement.Dtos.Product;
using StoreManagement.Exceptions;
using StoreManagement.Models;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public class ProductImageService : IProductImageService
{
    private const int MaxImagesPerProduct = 5;

    private readonly IProductImageRepository _productImageRepository;
    private readonly IProductRepository _productRepository;
    private readonly IFileStorageService _fileStorageService;

    public ProductImageService(IProductImageRepository productImageRepository, IProductRepository productRepository, IFileStorageService fileStorageService)
    {
        _productImageRepository = productImageRepository;
        _productRepository = productRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<List<ProductImageDto>> UploadProductImagesAsync(int productId, List<IFormFile> images, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, true, cancellationToken)
            ?? throw new ResourceNotFoundException($"Sản phẩm không tồn tại với ID: {productId}");

        var currentCount = await _productImageRepository.CountByProductIdAsync(productId, cancellationToken);
        if (currentCount + images.Count > MaxImagesPerProduct)
        {
            throw new InvalidOperationException($"Không thể upload. Sản phẩm chỉ được có tối đa {MaxImagesPerProduct} ảnh");
        }

        var results = new List<ProductImageDto>();
        foreach (var image in images)
        {
            results.Add(await AddProductImageAsync(productId, image, cancellationToken));
        }

        return results;
    }

    public async Task<ProductImageDto> AddProductImageAsync(int productId, IFormFile image, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, true, cancellationToken)
            ?? throw new ResourceNotFoundException($"Sản phẩm không tồn tại với ID: {productId}");

        var currentCount = await _productImageRepository.CountByProductIdAsync(productId, cancellationToken);
        if (currentCount >= MaxImagesPerProduct)
        {
            throw new InvalidOperationException($"Không thể thêm ảnh. Sản phẩm chỉ được có tối đa {MaxImagesPerProduct} ảnh");
        }

        var imageUrl = await _fileStorageService.SaveImageAsync(image, "products", cancellationToken);
        var entity = new ProductImages
        {
            IdProduct = productId,
            ImageUrl = imageUrl,
            IsPrimary = currentCount == 0,
            DisplayOrder = (int)currentCount
        };

        await _productImageRepository.AddAsync(entity, cancellationToken);
        if (entity.IsPrimary == true)
        {
            product.ImageUrl = imageUrl;
        }
        await _productImageRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(entity);
    }

    public async Task<List<ProductImageDto>> GetProductImagesAsync(int productId, CancellationToken cancellationToken = default)
    {
        _ = await _productRepository.GetByIdAsync(productId, true, cancellationToken)
            ?? throw new ResourceNotFoundException($"Sản phẩm không tồn tại với ID: {productId}");
        var images = await _productImageRepository.GetByProductIdAsync(productId, cancellationToken);
        return images.Select(MapToDto).ToList();
    }

    public async Task DeleteProductImageAsync(int imageId, CancellationToken cancellationToken = default)
    {
        var image = await _productImageRepository.GetByIdAsync(imageId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Ảnh không tồn tại với ID: {imageId}");

        var product = image.IdProductNavigation;
        var wasPrimary = image.IsPrimary == true;
        await _productImageRepository.DeleteAsync(image, cancellationToken);
        await _productImageRepository.SaveChangesAsync(cancellationToken);
        await _fileStorageService.DeleteImageAsync(image.ImageUrl, cancellationToken);

        if (wasPrimary)
        {
            var newPrimary = await _productImageRepository.GetFirstAsync(product.IdProduct, cancellationToken);
            if (newPrimary is not null)
            {
                newPrimary.IsPrimary = true;
                product.ImageUrl = newPrimary.ImageUrl;
            }
            else
            {
                product.ImageUrl = null;
            }

            await _productImageRepository.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<ProductImageDto> SetImageAsPrimaryAsync(int imageId, CancellationToken cancellationToken = default)
    {
        var image = await _productImageRepository.GetByIdAsync(imageId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Ảnh không tồn tại với ID: {imageId}");

        var currentPrimary = await _productImageRepository.GetPrimaryAsync(image.IdProduct, cancellationToken);
        if (currentPrimary is not null)
        {
            currentPrimary.IsPrimary = false;
        }

        image.IsPrimary = true;
        image.IdProductNavigation.ImageUrl = image.ImageUrl;
        await _productImageRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(image);
    }

    private static ProductImageDto MapToDto(ProductImages image)
    {
        return new ProductImageDto
        {
            IdProductImage = image.IdProductImage,
            IdProduct = image.IdProduct,
            ImageUrl = image.ImageUrl,
            IsPrimary = image.IsPrimary,
            DisplayOrder = image.DisplayOrder,
            CreatedAt = image.CreatedAt
        };
    }
}
