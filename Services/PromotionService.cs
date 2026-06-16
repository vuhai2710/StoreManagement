using StoreManagement.Common;
using StoreManagement.Dtos.Product;
using StoreManagement.Dtos.Promotion;
using StoreManagement.Exceptions;
using StoreManagement.Extensions;
using StoreManagement.Models;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IPromotionRuleRepository _promotionRuleRepository;

    public PromotionService(IPromotionRepository promotionRepository, IPromotionRuleRepository promotionRuleRepository)
    {
        _promotionRepository = promotionRepository;
        _promotionRuleRepository = promotionRuleRepository;
    }

    public async Task<List<ProductOnSaleDto>> GetProductsOnSaleAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var promotions = await _promotionRepository.GetActiveProductPromotionsAsync(now, cancellationToken);
        var results = new List<ProductOnSaleDto>();

        foreach (var promotion in promotions)
        {
            foreach (var product in promotion.IdProduct)
            {
                var discountedPrice = CalculateDiscountAmount(product.Price, promotion.DiscountType, promotion.DiscountValue, promotion.Scope == "SHIPPING" ? 0 : product.Price);
                var discountAmount = Math.Max(0, product.Price - discountedPrice);
                var percentage = product.Price > 0 ? (int)Math.Round(discountAmount / product.Price * 100m) : 0;
                results.Add(new ProductOnSaleDto
                {
                    ProductId = product.IdProduct,
                    Name = product.ProductName,
                    Image = product.ImageUrl,
                    OriginalPrice = product.Price,
                    DiscountedPrice = discountedPrice,
                    PromotionEndTime = promotion.EndDate,
                    DiscountLabel = promotion.DiscountType == "PERCENTAGE" ? $"-{promotion.DiscountValue:0}%" : $"-{promotion.DiscountValue:0.##}",
                    PromotionName = promotion.Code,
                    RemainingStock = product.StockQuantity,
                    DiscountPercentage = percentage
                });
            }
        }

        return results;
    }

    public async Task<ValidatePromotionResponseDto> ValidatePromotionAsync(ValidatePromotionRequestDto request, CancellationToken cancellationToken = default)
    {
        var promotion = await _promotionRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (promotion is null)
        {
            return new ValidatePromotionResponseDto
            {
                Valid = false,
                Message = "Mã giảm giá không tồn tại",
                Code = request.Code
            };
        }

        var now = DateTime.UtcNow;
        if (promotion.IsActive != true || promotion.StartDate > now || promotion.EndDate < now)
        {
            return new ValidatePromotionResponseDto
            {
                Valid = false,
                Message = "Mã giảm giá không còn hiệu lực",
                Code = request.Code,
                Scope = promotion.Scope
            };
        }

        if (promotion.UsageLimit.HasValue && (promotion.UsageCount ?? 0) >= promotion.UsageLimit.Value)
        {
            return new ValidatePromotionResponseDto
            {
                Valid = false,
                Message = "Mã giảm giá đã hết lượt sử dụng",
                Code = request.Code,
                Scope = promotion.Scope
            };
        }

        if (promotion.MinOrderAmount.HasValue && request.TotalAmount < promotion.MinOrderAmount.Value)
        {
            return new ValidatePromotionResponseDto
            {
                Valid = false,
                Message = "Đơn hàng chưa đạt giá trị tối thiểu để áp dụng mã",
                Code = request.Code,
                Scope = promotion.Scope
            };
        }

        if (!string.IsNullOrWhiteSpace(request.ExpectedScope) && !string.Equals(request.ExpectedScope, promotion.Scope, StringComparison.OrdinalIgnoreCase))
        {
            return new ValidatePromotionResponseDto
            {
                Valid = false,
                Message = "Mã giảm giá không áp dụng cho phạm vi mong muốn",
                Code = request.Code,
                Scope = promotion.Scope
            };
        }

        var baseAmount = promotion.Scope == "SHIPPING" ? request.ShippingFee ?? 0 : request.TotalAmount;
        var discount = CalculateDiscountAmount(baseAmount, promotion.DiscountType, promotion.DiscountValue, baseAmount);
        var discountValue = Math.Max(0, baseAmount - discount);

        return new ValidatePromotionResponseDto
        {
            Valid = true,
            Message = "Validate promotion code",
            Discount = discountValue,
            DiscountType = promotion.DiscountType,
            Code = promotion.Code,
            Scope = promotion.Scope
        };
    }

    public async Task<CalculateDiscountResponseDto> CalculateAutomaticDiscountAsync(CalculateDiscountRequestDto request, string customerType, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var rules = await _promotionRuleRepository.GetActiveRulesAsync("ORDER", now, cancellationToken);
        var rule = rules.FirstOrDefault(x =>
            (string.IsNullOrWhiteSpace(x.CustomerType) || string.Equals(x.CustomerType, customerType, StringComparison.OrdinalIgnoreCase)) &&
            request.TotalAmount >= (x.MinOrderAmount ?? 0));

        if (rule is null)
        {
            return new CalculateDiscountResponseDto
            {
                Applicable = false,
                Discount = 0
            };
        }

        var finalAmount = CalculateDiscountAmount(request.TotalAmount, rule.DiscountType, rule.DiscountValue, request.TotalAmount);
        return new CalculateDiscountResponseDto
        {
            Applicable = true,
            Discount = Math.Max(0, request.TotalAmount - finalAmount),
            DiscountType = rule.DiscountType,
            RuleName = rule.RuleName,
            RuleId = rule.IdRule
        };
    }

    public async Task<CalculateDiscountResponseDto> CalculateAutoShippingDiscountAsync(decimal? shippingFee, decimal totalAmount, string customerType, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var rules = await _promotionRuleRepository.GetActiveRulesAsync("SHIPPING", now, cancellationToken);
        var rule = rules.FirstOrDefault(x =>
            (string.IsNullOrWhiteSpace(x.CustomerType) || string.Equals(x.CustomerType, customerType, StringComparison.OrdinalIgnoreCase)) &&
            totalAmount >= (x.MinOrderAmount ?? 0));

        var fee = shippingFee ?? 0;
        if (rule is null || fee <= 0)
        {
            return new CalculateDiscountResponseDto
            {
                Applicable = false,
                Discount = 0
            };
        }

        var finalFee = CalculateDiscountAmount(fee, rule.DiscountType, rule.DiscountValue, fee);
        return new CalculateDiscountResponseDto
        {
            Applicable = true,
            Discount = Math.Max(0, fee - finalFee),
            DiscountType = rule.DiscountType,
            RuleName = rule.RuleName,
            RuleId = rule.IdRule
        };
    }

    public async Task<PageResponse<PromotionDto>> GetAllPromotionsAsync(string? keyword, string? scope, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var page = await _promotionRepository.SearchAsync(keyword, scope, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<PromotionDto>
        {
            Items = page.Items.Select(MapPromotion).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<PromotionDto> GetPromotionByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _promotionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Mã giảm giá không tồn tại với ID: {id}");
        return MapPromotion(entity);
    }

    public async Task<PromotionDto> CreatePromotionAsync(PromotionDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
        {
            throw new InvalidOperationException("Code không được để trống");
        }

        if (await _promotionRepository.GetByCodeAsync(dto.Code.Trim(), cancellationToken) is not null)
        {
            throw new ConflictException($"Mã giảm giá đã tồn tại: {dto.Code}");
        }

        var entity = new Promotions
        {
            Code = dto.Code.Trim(),
            DiscountType = dto.DiscountType ?? "PERCENTAGE",
            DiscountValue = dto.DiscountValue ?? 0,
            MinOrderAmount = dto.MinOrderAmount,
            UsageLimit = dto.UsageLimit,
            UsageCount = dto.UsageCount ?? 0,
            StartDate = dto.StartDate ?? DateTime.UtcNow,
            EndDate = dto.EndDate ?? DateTime.UtcNow.AddDays(30),
            IsActive = dto.IsActive ?? true,
            Scope = dto.Scope ?? "ORDER"
        };

        await _promotionRepository.AddAsync(entity, cancellationToken);
        await _promotionRepository.SaveChangesAsync(cancellationToken);
        return MapPromotion(entity);
    }

    public async Task<PromotionDto> UpdatePromotionAsync(int id, PromotionDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _promotionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Mã giảm giá không tồn tại với ID: {id}");

        if (!string.IsNullOrWhiteSpace(dto.Code) && !string.Equals(entity.Code, dto.Code.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            if (await _promotionRepository.GetByCodeAsync(dto.Code.Trim(), cancellationToken) is not null)
            {
                throw new ConflictException($"Mã giảm giá đã tồn tại: {dto.Code}");
            }
            entity.Code = dto.Code.Trim();
        }

        entity.DiscountType = dto.DiscountType ?? entity.DiscountType;
        entity.DiscountValue = dto.DiscountValue ?? entity.DiscountValue;
        entity.MinOrderAmount = dto.MinOrderAmount ?? entity.MinOrderAmount;
        entity.UsageLimit = dto.UsageLimit ?? entity.UsageLimit;
        entity.UsageCount = dto.UsageCount ?? entity.UsageCount;
        entity.StartDate = dto.StartDate ?? entity.StartDate;
        entity.EndDate = dto.EndDate ?? entity.EndDate;
        entity.IsActive = dto.IsActive ?? entity.IsActive;
        entity.Scope = dto.Scope ?? entity.Scope;
        await _promotionRepository.SaveChangesAsync(cancellationToken);
        return MapPromotion(entity);
    }

    public async Task DeletePromotionAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _promotionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Mã giảm giá không tồn tại với ID: {id}");
        await _promotionRepository.DeleteAsync(entity, cancellationToken);
        await _promotionRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<PageResponse<PromotionRuleDto>> GetAllPromotionRulesAsync(int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var page = await _promotionRuleRepository.GetPagedAsync(pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<PromotionRuleDto>
        {
            Items = page.Items.Select(MapPromotionRule).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<PromotionRuleDto> GetPromotionRuleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _promotionRuleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Quy tắc giảm giá không tồn tại với ID: {id}");
        return MapPromotionRule(entity);
    }

    public async Task<PromotionRuleDto> CreatePromotionRuleAsync(PromotionRuleDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.RuleName))
        {
            throw new InvalidOperationException("Rule name không được để trống");
        }

        var entity = new PromotionRules
        {
            RuleName = dto.RuleName.Trim(),
            DiscountType = dto.DiscountType ?? "PERCENTAGE",
            DiscountValue = dto.DiscountValue ?? 0,
            MinOrderAmount = dto.MinOrderAmount,
            CustomerType = dto.CustomerType,
            StartDate = dto.StartDate ?? DateTime.UtcNow,
            EndDate = dto.EndDate ?? DateTime.UtcNow.AddDays(30),
            IsActive = dto.IsActive ?? true,
            Priority = dto.Priority ?? 0,
            Scope = dto.Scope ?? "ORDER"
        };

        await _promotionRuleRepository.AddAsync(entity, cancellationToken);
        await _promotionRuleRepository.SaveChangesAsync(cancellationToken);
        return MapPromotionRule(entity);
    }

    public async Task<PromotionRuleDto> UpdatePromotionRuleAsync(int id, PromotionRuleDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _promotionRuleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Quy tắc giảm giá không tồn tại với ID: {id}");

        entity.RuleName = dto.RuleName ?? entity.RuleName;
        entity.DiscountType = dto.DiscountType ?? entity.DiscountType;
        entity.DiscountValue = dto.DiscountValue ?? entity.DiscountValue;
        entity.MinOrderAmount = dto.MinOrderAmount ?? entity.MinOrderAmount;
        entity.CustomerType = dto.CustomerType ?? entity.CustomerType;
        entity.StartDate = dto.StartDate ?? entity.StartDate;
        entity.EndDate = dto.EndDate ?? entity.EndDate;
        entity.IsActive = dto.IsActive ?? entity.IsActive;
        entity.Priority = dto.Priority ?? entity.Priority;
        entity.Scope = dto.Scope ?? entity.Scope;
        await _promotionRuleRepository.SaveChangesAsync(cancellationToken);
        return MapPromotionRule(entity);
    }

    public async Task DeletePromotionRuleAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _promotionRuleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Quy tắc giảm giá không tồn tại với ID: {id}");
        await _promotionRuleRepository.DeleteAsync(entity, cancellationToken);
        await _promotionRuleRepository.SaveChangesAsync(cancellationToken);
    }

    private static decimal CalculateDiscountAmount(decimal baseAmount, string? discountType, decimal discountValue, decimal maxBase)
    {
        if (discountType == "PERCENTAGE")
        {
            var reduced = baseAmount - (baseAmount * discountValue / 100m);
            return reduced < 0 ? 0 : reduced;
        }

        if (discountType == "FIXED_AMOUNT")
        {
            var reduced = baseAmount - discountValue;
            return reduced < 0 ? 0 : reduced;
        }

        return maxBase;
    }

    private static PromotionDto MapPromotion(Promotions entity) => new()
    {
        IdPromotion = entity.IdPromotion,
        Code = entity.Code,
        DiscountType = entity.DiscountType,
        DiscountValue = entity.DiscountValue,
        MinOrderAmount = entity.MinOrderAmount,
        UsageLimit = entity.UsageLimit,
        UsageCount = entity.UsageCount,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        IsActive = entity.IsActive,
        Scope = entity.Scope,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };

    private static PromotionRuleDto MapPromotionRule(PromotionRules entity) => new()
    {
        IdRule = entity.IdRule,
        RuleName = entity.RuleName,
        DiscountType = entity.DiscountType,
        DiscountValue = entity.DiscountValue,
        MinOrderAmount = entity.MinOrderAmount,
        CustomerType = entity.CustomerType,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        IsActive = entity.IsActive,
        Priority = entity.Priority,
        Scope = entity.Scope,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}