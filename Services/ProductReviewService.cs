using StoreManagement.Common;
using StoreManagement.Dtos.Review;
using StoreManagement.Exceptions;
using StoreManagement.Extensions;
using StoreManagement.Models;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public interface IProductReviewService
{
    Task<ProductReviewDto> CreateReviewAsync(int customerId, int productId, CreateReviewRequestDto request, CancellationToken cancellationToken = default);
    Task<PageResponse<ProductReviewDto>> GetProductReviewsAsync(int productId, int? rating, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<PageResponse<ProductReviewDto>> GetMyReviewsAsync(int customerId, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<ProductReviewDto> UpdateReviewAsync(int customerId, int reviewId, UpdateReviewRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteReviewAsync(int customerId, int reviewId, CancellationToken cancellationToken = default);
    Task<PageResponse<ProductReviewDto>> GetAllReviewsAsync(int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<ProductReviewDto> GetReviewByIdAsync(int reviewId, CancellationToken cancellationToken = default);
    Task DeleteReviewByAdminAsync(int reviewId, CancellationToken cancellationToken = default);
    Task<ProductReviewDto> ReplyToReviewAsync(int reviewId, string adminReply, CancellationToken cancellationToken = default);
}

public class ProductReviewService : IProductReviewService
{
    private readonly IProductReviewRepository _productReviewRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly ISystemSettingService _systemSettingService;

    public ProductReviewService(
        IProductReviewRepository productReviewRepository,
        IProductRepository productRepository,
        ICustomerRepository customerRepository,
        IOrderDetailRepository orderDetailRepository,
        ISystemSettingService systemSettingService)
    {
        _productReviewRepository = productReviewRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _orderDetailRepository = orderDetailRepository;
        _systemSettingService = systemSettingService;
    }

    public async Task<ProductReviewDto> CreateReviewAsync(int customerId, int productId, CreateReviewRequestDto request, CancellationToken cancellationToken = default)
    {
        _ = await _customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy khách hàng");
        _ = await _productRepository.GetByIdAsync(productId, false, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy sản phẩm");
        var detail = await _orderDetailRepository.GetByIdAsync(request.OrderDetailId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy chi tiết đơn hàng");

        if (detail.IdOrderNavigation.IdCustomer != customerId)
        {
            throw new InvalidOperationException("Chi tiết đơn hàng không thuộc về khách hàng này");
        }

        if (detail.IdProduct != productId)
        {
            throw new InvalidOperationException("Chi tiết đơn hàng không thuộc về sản phẩm này");
        }

        if (!string.Equals(detail.IdOrderNavigation.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Chỉ có thể đánh giá sản phẩm từ đơn hàng đã hoàn thành");
        }

        if (await _productReviewRepository.GetByOrderDetailIdAsync(request.OrderDetailId, cancellationToken) is not null)
        {
            throw new InvalidOperationException("Đơn hàng này đã được đánh giá");
        }

        var entity = new ProductReviews
        {
            IdProduct = productId,
            IdCustomer = customerId,
            IdOrder = detail.IdOrder,
            IdOrderDetail = detail.IdOrderDetail,
            Rating = request.Rating,
            Comment = request.Comment.Trim()
        };

        await _productReviewRepository.AddAsync(entity, cancellationToken);
        await _productReviewRepository.SaveChangesAsync(cancellationToken);
        return await GetReviewByIdAsync(entity.IdReview, cancellationToken);
    }

    public async Task<PageResponse<ProductReviewDto>> GetProductReviewsAsync(int productId, int? rating, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var page = await _productReviewRepository.GetByProductAsync(productId, rating, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<ProductReviewDto>
        {
            Items = page.Items.Select(Map).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<PageResponse<ProductReviewDto>> GetMyReviewsAsync(int customerId, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var page = await _productReviewRepository.GetByCustomerAsync(customerId, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<ProductReviewDto>
        {
            Items = page.Items.Select(Map).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<ProductReviewDto> UpdateReviewAsync(int customerId, int reviewId, UpdateReviewRequestDto request, CancellationToken cancellationToken = default)
    {
        var entity = await _productReviewRepository.GetByIdAsync(reviewId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy đánh giá");
        if (entity.IdCustomer != customerId)
        {
            throw new UnauthorizedAccessException("Không có quyền chỉnh sửa đánh giá này");
        }

        var editWindow = await _systemSettingService.GetReviewEditWindowHoursAsync(cancellationToken);
        if (entity.CreatedAt.HasValue && entity.CreatedAt.Value.AddHours(editWindow) <= DateTime.UtcNow)
        {
            throw new InvalidOperationException($"Chỉ có thể chỉnh sửa đánh giá trong vòng {editWindow} giờ sau khi tạo");
        }

        if (entity.EditCount >= 1)
        {
            throw new InvalidOperationException("Chỉ được phép chỉnh sửa đánh giá 1 lần");
        }

        entity.Rating = request.Rating;
        entity.Comment = request.Comment.Trim();
        entity.EditCount += 1;
        await _productReviewRepository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task DeleteReviewAsync(int customerId, int reviewId, CancellationToken cancellationToken = default)
    {
        var entity = await _productReviewRepository.GetByIdAsync(reviewId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy đánh giá");
        if (entity.IdCustomer != customerId)
        {
            throw new UnauthorizedAccessException("Không có quyền xóa đánh giá này");
        }

        await _productReviewRepository.DeleteAsync(entity, cancellationToken);
        await _productReviewRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<PageResponse<ProductReviewDto>> GetAllReviewsAsync(int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var page = await _productReviewRepository.GetAllPagedAsync(pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<ProductReviewDto>
        {
            Items = page.Items.Select(Map).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<ProductReviewDto> GetReviewByIdAsync(int reviewId, CancellationToken cancellationToken = default)
    {
        var entity = await _productReviewRepository.GetByIdAsync(reviewId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy đánh giá");
        return Map(entity);
    }

    public async Task DeleteReviewByAdminAsync(int reviewId, CancellationToken cancellationToken = default)
    {
        var entity = await _productReviewRepository.GetByIdAsync(reviewId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy đánh giá");
        await _productReviewRepository.DeleteAsync(entity, cancellationToken);
        await _productReviewRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductReviewDto> ReplyToReviewAsync(int reviewId, string adminReply, CancellationToken cancellationToken = default)
    {
        var entity = await _productReviewRepository.GetByIdAsync(reviewId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy đánh giá");
        entity.AdminReply = adminReply.Trim();
        await _productReviewRepository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    private static ProductReviewDto Map(ProductReviews entity)
    {
        return new ProductReviewDto
        {
            IdReview = entity.IdReview,
            IdProduct = entity.IdProduct,
            IdCustomer = entity.IdCustomer,
            CustomerName = entity.IdCustomerNavigation?.CustomerName,
            IdOrder = entity.IdOrder,
            IdOrderDetail = entity.IdOrderDetail,
            Rating = entity.Rating,
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            AdminReply = entity.AdminReply,
            EditCount = entity.EditCount
        };
    }
}