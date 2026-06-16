using StoreManagement.Common;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface IPromotionRepository
{
    Task<Promotions?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<Promotions>> GetActiveProductPromotionsAsync(DateTime now, CancellationToken cancellationToken = default);
    Task<PagedResult<Promotions>> SearchAsync(string? keyword, string? scope, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<Promotions?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Promotions promotion, CancellationToken cancellationToken = default);
    Task DeleteAsync(Promotions promotion, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}