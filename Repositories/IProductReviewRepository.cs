using StoreManagement.Common;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface IProductReviewRepository
{
    Task<(double AverageRating, int ReviewCount)> GetSummaryAsync(int productId, CancellationToken cancellationToken = default);
    Task<ProductReviews?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductReviews?> GetByOrderDetailIdAsync(int orderDetailId, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductReviews>> GetByProductAsync(int productId, int? rating, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductReviews>> GetByCustomerAsync(int customerId, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductReviews>> GetAllPagedAsync(int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task AddAsync(ProductReviews review, CancellationToken cancellationToken = default);
    Task DeleteAsync(ProductReviews review, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}