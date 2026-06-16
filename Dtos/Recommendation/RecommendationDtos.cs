using StoreManagement.Dtos.Product;

namespace StoreManagement.Dtos.Recommendation;

public class ProductRecommendationResponseDto
{
    public List<ProductDto> MostViewedProducts { get; set; } = [];
    public List<ProductDto> RecommendedProducts { get; set; } = [];
    public List<ProductDto> AllProducts { get; set; } = [];
}