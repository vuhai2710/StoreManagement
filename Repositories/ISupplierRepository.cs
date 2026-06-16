using StoreManagement.Common;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface ISupplierRepository
{
    Task<Suppliers?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Suppliers?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<List<Suppliers>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<Suppliers>> GetPagedAsync(int pageNo, int pageSize, string sortBy, string sortDirection, string? keyword, CancellationToken cancellationToken = default);
    Task AddAsync(Suppliers supplier, CancellationToken cancellationToken = default);
    Task DeleteAsync(Suppliers supplier, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
