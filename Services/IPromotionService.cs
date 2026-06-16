using StoreManagement.Common;
using StoreManagement.Dtos.Product;
using StoreManagement.Dtos.Promotion;

namespace StoreManagement.Services;

public interface IPromotionService
{
    Task<List<ProductOnSaleDto>> GetProductsOnSaleAsync(CancellationToken cancellationToken = default);
    Task<ValidatePromotionResponseDto> ValidatePromotionAsync(ValidatePromotionRequestDto request, CancellationToken cancellationToken = default);
    Task<CalculateDiscountResponseDto> CalculateAutomaticDiscountAsync(CalculateDiscountRequestDto request, string customerType, CancellationToken cancellationToken = default);
    Task<CalculateDiscountResponseDto> CalculateAutoShippingDiscountAsync(decimal? shippingFee, decimal totalAmount, string customerType, CancellationToken cancellationToken = default);
    Task<PageResponse<PromotionDto>> GetAllPromotionsAsync(string? keyword, string? scope, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<PromotionDto> GetPromotionByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PromotionDto> CreatePromotionAsync(PromotionDto dto, CancellationToken cancellationToken = default);
    Task<PromotionDto> UpdatePromotionAsync(int id, PromotionDto dto, CancellationToken cancellationToken = default);
    Task DeletePromotionAsync(int id, CancellationToken cancellationToken = default);
    Task<PageResponse<PromotionRuleDto>> GetAllPromotionRulesAsync(int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<PromotionRuleDto> GetPromotionRuleByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PromotionRuleDto> CreatePromotionRuleAsync(PromotionRuleDto dto, CancellationToken cancellationToken = default);
    Task<PromotionRuleDto> UpdatePromotionRuleAsync(int id, PromotionRuleDto dto, CancellationToken cancellationToken = default);
    Task DeletePromotionRuleAsync(int id, CancellationToken cancellationToken = default);
}