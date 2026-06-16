using Microsoft.EntityFrameworkCore;
using StoreManagement.Common;
using StoreManagement.Data;
using StoreManagement.Extensions;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _dbContext;

    public EmployeeRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int?> GetEmployeeIdByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Employees
            .Where(x => x.IdUser == userId)
            .Select(x => (int?)x.IdEmployee)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Employees?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Employees
            .Include(x => x.IdUserNavigation)
            .FirstOrDefaultAsync(x => x.IdEmployee == id, cancellationToken);
    }

    public Task<Employees?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Employees
            .Include(x => x.IdUserNavigation)
            .FirstOrDefaultAsync(x => x.IdUser == userId, cancellationToken);
    }

    public Task<Employees?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return _dbContext.Employees
            .Include(x => x.IdUserNavigation)
            .FirstOrDefaultAsync(x => x.IdUserNavigation != null && x.IdUserNavigation.Username == username, cancellationToken);
    }

    public Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return _dbContext.Employees.AnyAsync(x => x.PhoneNumber == phoneNumber && (!excludeId.HasValue || x.IdEmployee != excludeId.Value), cancellationToken);
    }

    public Task<List<Employees>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Employees.Include(x => x.IdUserNavigation).AsNoTracking().OrderBy(x => x.IdEmployee).ToListAsync(cancellationToken);
    }

    public Task<PagedResult<Employees>> GetPagedAsync(string? keyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Employees
            .AsNoTracking()
            .Include(x => x.IdUserNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalized = keyword.Trim().ToLower();
            query = query.Where(x =>
                x.EmployeeName.ToLower().Contains(normalized) ||
                (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(normalized)) ||
                (x.IdUserNavigation != null && (
                    x.IdUserNavigation.Email.ToLower().Contains(normalized) ||
                    x.IdUserNavigation.Username.ToLower().Contains(normalized))));
        }

        return query.ApplySorting(sortBy, sortDirection).ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public async Task AddAsync(Employees employee, CancellationToken cancellationToken = default)
    {
        await _dbContext.Employees.AddAsync(employee, cancellationToken);
    }

    public Task DeleteAsync(Employees employee, CancellationToken cancellationToken = default)
    {
        _dbContext.Employees.Remove(employee);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
