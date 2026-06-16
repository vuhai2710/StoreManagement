using StoreManagement.Common;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface ICustomerRepository
{
    Task<Customers?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Customers?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Customers?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Customers?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<PagedResult<Customers>> GetPagedAsync(string? keyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<PagedResult<Customers>> SearchAsync(string? name, string? phone, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<PagedResult<Customers>> GetByTypePagedAsync(string type, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task AddAsync(Customers customer, CancellationToken cancellationToken = default);
    Task DeleteAsync(Customers customer, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
