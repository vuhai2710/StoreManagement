using Microsoft.EntityFrameworkCore;
using StoreManagement.Common;
using StoreManagement.Data;
using StoreManagement.Extensions;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class PromotionRepository : IPromotionRepository
{
    private readonly AppDbContext _dbContext;

    public PromotionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Promotions?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return _dbContext.Promotions.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
    }

    public Task<List<Promotions>> GetActiveProductPromotionsAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        return _dbContext.Promotions
            .Include(x => x.IdProduct)
            .Where(x => x.IsActive == true && x.Scope == "PRODUCT" && x.StartDate <= now && x.EndDate >= now)
            .ToListAsync(cancellationToken);
    }

    public Task<PagedResult<Promotions>> SearchAsync(string? keyword, string? scope, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Promotions.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim();
            query = query.Where(x => x.Code.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(scope))
        {
            query = query.Where(x => x.Scope == scope);
        }

        return query.ApplySorting(sortBy, sortDirection).ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public Task<Promotions?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Promotions.FirstOrDefaultAsync(x => x.IdPromotion == id, cancellationToken);

    public Task AddAsync(Promotions promotion, CancellationToken cancellationToken = default) =>
        _dbContext.Promotions.AddAsync(promotion, cancellationToken).AsTask();

    public Task DeleteAsync(Promotions promotion, CancellationToken cancellationToken = default)
    {
        _dbContext.Promotions.Remove(promotion);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}