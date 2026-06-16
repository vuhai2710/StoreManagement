using StoreManagement.Common;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface ICategoryRepository
{
    Task<Categories?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Categories?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<Categories>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<Categories>> GetPagedAsync(int pageNo, int pageSize, string sortBy, string sortDirection, string? name, CancellationToken cancellationToken = default);
    Task AddAsync(Categories category, CancellationToken cancellationToken = default);
    Task DeleteAsync(Categories category, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
