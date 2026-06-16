using Microsoft.EntityFrameworkCore;
using StoreManagement.Common;
using StoreManagement.Data;
using StoreManagement.Extensions;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class PromotionRuleRepository : IPromotionRuleRepository
{
    private readonly AppDbContext _dbContext;

    public PromotionRuleRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<PromotionRules>> GetActiveRulesAsync(string scope, DateTime now, CancellationToken cancellationToken = default)
    {
        return _dbContext.PromotionRules
            .Where(x => x.IsActive == true && x.Scope == scope && x.StartDate <= now && x.EndDate >= now)
            .OrderByDescending(x => x.Priority ?? 0)
            .ToListAsync(cancellationToken);
    }

    public Task<PagedResult<PromotionRules>> GetPagedAsync(int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default) =>
        _dbContext.PromotionRules.AsNoTracking().ApplySorting(sortBy, sortDirection).ToPagedResultAsync(pageNo, pageSize, cancellationToken);

    public Task<PromotionRules?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.PromotionRules.FirstOrDefaultAsync(x => x.IdRule == id, cancellationToken);

    public Task AddAsync(PromotionRules rule, CancellationToken cancellationToken = default) =>
        _dbContext.PromotionRules.AddAsync(rule, cancellationToken).AsTask();

    public Task DeleteAsync(PromotionRules rule, CancellationToken cancellationToken = default)
    {
        _dbContext.PromotionRules.Remove(rule);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}