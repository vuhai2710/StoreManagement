using StoreManagement.Dtos.Product;
using StoreManagement.Dtos.Recommendation;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public interface IProductRecommendationService
{
    Task<List<ProductDto>> RecommendForUserAsync(int? userId, CancellationToken cancellationToken = default);
    Task<ProductRecommendationResponseDto> RecommendForUserWithMetadataAsync(int? userId, CancellationToken cancellationToken = default);
    Task<List<ProductDto>> SimilarProductsAsync(long productId, CancellationToken cancellationToken = default);
}

public class ProductRecommendationService : IProductRecommendationService
{
    private readonly IProductViewRepository _productViewRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IProductService _productService;

    public ProductRecommendationService(
        IProductViewRepository productViewRepository,
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IProductService productService)
    {
        _productViewRepository = productViewRepository;
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _productService = productService;
    }

    public async Task<List<ProductDto>> RecommendForUserAsync(int? userId, CancellationToken cancellationToken = default)
    {
        var response = await RecommendForUserWithMetadataAsync(userId, cancellationToken);
        return response.AllProducts;
    }

    public async Task<ProductRecommendationResponseDto> RecommendForUserWithMetadataAsync(int? userId, CancellationToken cancellationToken = default)
    {
        int? customerId = null;
        if (userId.HasValue)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId.Value, cancellationToken);
            customerId = customer?.IdCustomer;
        }

        var excluded = new HashSet<int>();
        if (customerId.HasValue)
        {
            var purchased = await _orderRepository.FindPurchasedProductIdsByCustomerIdAsync(customerId.Value, cancellationToken);
            foreach (var item in purchased)
            {
                excluded.Add(item);
            }

            var cart = await _cartRepository.GetByCustomerIdAsync(customerId.Value, cancellationToken);
            if (cart?.CartItems is not null)
            {
                foreach (var item in cart.CartItems)
                {
                    excluded.Add(item.IdProduct);
                }
            }
        }

        var mostViewedIds = (await _productViewRepository.GetTopViewedProductIdsAsync(userId, 12, cancellationToken))
            .Where(id => !excluded.Contains(id))
            .Take(4)
            .ToList();

        var mostViewed = await LoadProductsAsync(mostViewedIds, cancellationToken);
        var bestSelling = await _productService.GetTop5BestSellingProductsAsync(null, false, cancellationToken);
        var recommended = bestSelling.Where(x => x.IdProduct.HasValue && !excluded.Contains(x.IdProduct.Value) && !mostViewedIds.Contains(x.IdProduct.Value)).Take(8).ToList();

        var all = new List<ProductDto>();
        all.AddRange(mostViewed);
        all.AddRange(recommended);

        return new ProductRecommendationResponseDto
        {
            MostViewedProducts = mostViewed,
            RecommendedProducts = recommended,
            AllProducts = all.Take(12).ToList()
        };
    }

    public async Task<List<ProductDto>> SimilarProductsAsync(long productId, CancellationToken cancellationToken = default)
    {
        var product = await _productService.GetProductByIdAsync((int)productId, false, cancellationToken);
        if (!product.IdCategory.HasValue)
        {
            return [];
        }

        var page = await _productService.GetProductsByCategoryAsync(product.IdCategory.Value, 1, 8, "createdAt", "DESC", false, cancellationToken);
        return page.Content.Where(x => x.IdProduct != productId).Take(8).ToList();
    }

    private async Task<List<ProductDto>> LoadProductsAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
    {
        var result = new List<ProductDto>();
        foreach (var id in ids)
        {
            result.Add(await _productService.GetProductByIdAsync(id, false, cancellationToken));
        }
        return result;
    }
}