using StoreManagement.Common;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface IPromotionRuleRepository
{
    Task<List<PromotionRules>> GetActiveRulesAsync(string scope, DateTime now, CancellationToken cancellationToken = default);
    Task<PagedResult<PromotionRules>> GetPagedAsync(int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<PromotionRules?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(PromotionRules rule, CancellationToken cancellationToken = default);
    Task DeleteAsync(PromotionRules rule, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}