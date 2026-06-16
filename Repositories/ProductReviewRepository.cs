using Microsoft.EntityFrameworkCore;
using StoreManagement.Common;
using StoreManagement.Data;
using StoreManagement.Extensions;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class ProductReviewRepository : IProductReviewRepository
{
    private readonly AppDbContext _dbContext;

    public ProductReviewRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(double AverageRating, int ReviewCount)> GetSummaryAsync(int productId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ProductReviews.Where(x => x.IdProduct == productId);
        var reviewCount = await query.CountAsync(cancellationToken);
        if (reviewCount == 0)
        {
            return (0d, 0);
        }

        var average = await query.AverageAsync(x => (double)x.Rating, cancellationToken);
        return (average, reviewCount);
    }

    public Task<ProductReviews?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.ProductReviews
            .Include(x => x.IdCustomerNavigation)
            .Include(x => x.IdProductNavigation)
            .Include(x => x.IdOrderNavigation)
            .Include(x => x.IdOrderDetailNavigation)
            .FirstOrDefaultAsync(x => x.IdReview == id, cancellationToken);

    public Task<ProductReviews?> GetByOrderDetailIdAsync(int orderDetailId, CancellationToken cancellationToken = default) =>
        _dbContext.ProductReviews.FirstOrDefaultAsync(x => x.IdOrderDetail == orderDetailId, cancellationToken);

    public Task<PagedResult<ProductReviews>> GetByProductAsync(int productId, int? rating, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ProductReviews
            .AsNoTracking()
            .Include(x => x.IdCustomerNavigation)
            .Where(x => x.IdProduct == productId);

        if (rating.HasValue)
        {
            query = query.Where(x => x.Rating == rating.Value);
        }

        return query.ApplySorting(sortBy, sortDirection).ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public Task<PagedResult<ProductReviews>> GetByCustomerAsync(int customerId, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default) =>
        _dbContext.ProductReviews
            .AsNoTracking()
            .Include(x => x.IdCustomerNavigation)
            .Include(x => x.IdProductNavigation)
            .Where(x => x.IdCustomer == customerId)
            .ApplySorting(sortBy, sortDirection)
            .ToPagedResultAsync(pageNo, pageSize, cancellationToken);

    public Task<PagedResult<ProductReviews>> GetAllPagedAsync(int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default) =>
        _dbContext.ProductReviews
            .AsNoTracking()
            .Include(x => x.IdCustomerNavigation)
            .Include(x => x.IdProductNavigation)
            .ApplySorting(sortBy, sortDirection)
            .ToPagedResultAsync(pageNo, pageSize, cancellationToken);

    public Task AddAsync(ProductReviews review, CancellationToken cancellationToken = default) =>
        _dbContext.ProductReviews.AddAsync(review, cancellationToken).AsTask();

    public Task DeleteAsync(ProductReviews review, CancellationToken cancellationToken = default)
    {
        _dbContext.ProductReviews.Remove(review);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}