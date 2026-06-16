using StoreManagement.Common;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface IEmployeeRepository
{
    Task<int?> GetEmployeeIdByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Employees?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Employees?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Employees?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<List<Employees>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<Employees>> GetPagedAsync(string? keyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task AddAsync(Employees employee, CancellationToken cancellationToken = default);
    Task DeleteAsync(Employees employee, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
