using StoreManagement.Common;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface IUserRepository
{
    Task<Users?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Users?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Users?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<PagedResult<Users>> GetPagedAsync(int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<PagedResult<Users>> GetByStatusPagedAsync(bool isActive, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task AddAsync(Users user, CancellationToken cancellationToken = default);
    Task DeleteAsync(Users user, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
