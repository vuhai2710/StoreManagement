using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Product;
using StoreManagement.Dtos.Recommendation;
using StoreManagement.Dtos.Review;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1")]
public class ProductReviewController : ControllerBase
{
    private readonly IProductReviewService _productReviewService;
    private readonly ICustomerService _customerService;
    private readonly ICurrentUserContext _currentUserContext;

    public ProductReviewController(IProductReviewService productReviewService, ICustomerService customerService, ICurrentUserContext currentUserContext)
    {
        _productReviewService = productReviewService;
        _customerService = customerService;
        _currentUserContext = currentUserContext;
    }

    [HttpPost("products/{productId:int}/reviews")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<ProductReviewDto>>> CreateReview(int productId, [FromBody] CreateReviewRequestDto request, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetCustomerByUsernameAsync(_currentUserContext.GetRequiredUsername(), cancellationToken);
        var result = await _productReviewService.CreateReviewAsync(customer.IdCustomer!.Value, productId, request, cancellationToken);
        return Ok(ApiResponse<ProductReviewDto>.Success("Tạo đánh giá thành công", result));
    }

    [HttpGet("products/{productId:int}/reviews")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<PageResponse<ProductReviewDto>>>> GetProductReviews(int productId, [FromQuery] int? rating = null, [FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "CreatedAt", [FromQuery] string sortDirection = "DESC", CancellationToken cancellationToken = default)
    {
        var result = await _productReviewService.GetProductReviewsAsync(productId, rating, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<ProductReviewDto>>.Success("Lấy danh sách đánh giá thành công", result));
    }

    [HttpGet("reviews/my-reviews")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<PageResponse<ProductReviewDto>>>> GetMyReviews([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "CreatedAt", [FromQuery] string sortDirection = "DESC", CancellationToken cancellationToken = default)
    {
        var customer = await _customerService.GetCustomerByUsernameAsync(_currentUserContext.GetRequiredUsername(), cancellationToken);
        var result = await _productReviewService.GetMyReviewsAsync(customer.IdCustomer!.Value, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<ProductReviewDto>>.Success("Lấy danh sách đánh giá thành công", result));
    }

    [HttpPut("reviews/{reviewId:int}")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<ProductReviewDto>>> UpdateReview(int reviewId, [FromBody] UpdateReviewRequestDto request, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetCustomerByUsernameAsync(_currentUserContext.GetRequiredUsername(), cancellationToken);
        var result = await _productReviewService.UpdateReviewAsync(customer.IdCustomer!.Value, reviewId, request, cancellationToken);
        return Ok(ApiResponse<ProductReviewDto>.Success("Cập nhật đánh giá thành công", result));
    }

    [HttpDelete("reviews/{reviewId:int}")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteReview(int reviewId, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetCustomerByUsernameAsync(_currentUserContext.GetRequiredUsername(), cancellationToken);
        await _productReviewService.DeleteReviewAsync(customer.IdCustomer!.Value, reviewId, cancellationToken);
        return Ok(ApiResponse<object>.Success("Xóa đánh giá thành công", null));
    }

    [HttpGet("admin/reviews")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<ProductReviewDto>>>> GetAllReviews([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "CreatedAt", [FromQuery] string sortDirection = "DESC", CancellationToken cancellationToken = default)
    {
        var result = await _productReviewService.GetAllReviewsAsync(pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<ProductReviewDto>>.Success("Lấy danh sách đánh giá thành công", result));
    }

    [HttpGet("admin/reviews/{reviewId:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<ProductReviewDto>>> GetReviewById(int reviewId, CancellationToken cancellationToken)
    {
        var result = await _productReviewService.GetReviewByIdAsync(reviewId, cancellationToken);
        return Ok(ApiResponse<ProductReviewDto>.Success("Lấy chi tiết đánh giá thành công", result));
    }

    [HttpDelete("admin/reviews/{reviewId:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteReviewByAdmin(int reviewId, CancellationToken cancellationToken)
    {
        await _productReviewService.DeleteReviewByAdminAsync(reviewId, cancellationToken);
        return Ok(ApiResponse<object>.Success("Xóa đánh giá thành công", null));
    }

    [HttpPost("admin/reviews/{reviewId:int}/reply")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<ProductReviewDto>>> ReplyToReview(int reviewId, [FromBody] AdminReplyRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _productReviewService.ReplyToReviewAsync(reviewId, request.AdminReply, cancellationToken);
        return Ok(ApiResponse<ProductReviewDto>.Success("Trả lời đánh giá thành công", result));
    }
}

[ApiController]
[Route("api/v1/products")]
public class ProductRecommendationController : ControllerBase
{
    private readonly IProductRecommendationService _productRecommendationService;
    private readonly ICurrentUserContext _currentUserContext;

    public ProductRecommendationController(IProductRecommendationService productRecommendationService, ICurrentUserContext currentUserContext)
    {
        _productRecommendationService = productRecommendationService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet("recommend")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetRecommendations(CancellationToken cancellationToken)
    {
        var result = await _productRecommendationService.RecommendForUserAsync(_currentUserContext.GetRequiredUserId(), cancellationToken);
        return Ok(ApiResponse<List<ProductDto>>.Success("Lấy danh sách gợi ý sản phẩm thành công", result));
    }

    [HttpGet("recommend-with-metadata")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<ProductRecommendationResponseDto>>> GetRecommendationsWithMetadata(CancellationToken cancellationToken)
    {
        var result = await _productRecommendationService.RecommendForUserWithMetadataAsync(_currentUserContext.GetRequiredUserId(), cancellationToken);
        return Ok(ApiResponse<ProductRecommendationResponseDto>.Success("Lấy danh sách gợi ý sản phẩm thành công", result));
    }

    [HttpGet("{id:int}/similar")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetSimilarProducts(int id, CancellationToken cancellationToken)
    {
        var result = await _productRecommendationService.SimilarProductsAsync(id, cancellationToken);
        return Ok(ApiResponse<List<ProductDto>>.Success("Lấy danh sách sản phẩm tương tự thành công", result));
    }
}