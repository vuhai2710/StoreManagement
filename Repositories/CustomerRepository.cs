using Microsoft.EntityFrameworkCore;
using StoreManagement.Common;
using StoreManagement.Data;
using StoreManagement.Extensions;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _dbContext;

    public CustomerRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Customers?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers
            .Include(x => x.IdUserNavigation)
            .FirstOrDefaultAsync(x => x.IdCustomer == id, cancellationToken);
    }

    public Task<Customers?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers
            .Include(x => x.IdUserNavigation)
            .FirstOrDefaultAsync(x => x.IdUser == userId, cancellationToken);
    }

    public Task<Customers?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers
            .Include(x => x.IdUserNavigation)
            .FirstOrDefaultAsync(x => x.IdUserNavigation != null && x.IdUserNavigation.Username == username, cancellationToken);
    }

    public Task<Customers?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);
    }

    public Task<PagedResult<Customers>> GetPagedAsync(string? keyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Customers
            .AsNoTracking()
            .Include(x => x.IdUserNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalized = keyword.Trim().ToLower();
            query = query.Where(x =>
                x.CustomerName.ToLower().Contains(normalized) ||
                x.PhoneNumber.ToLower().Contains(normalized) ||
                (x.IdUserNavigation != null && x.IdUserNavigation.Email.ToLower().Contains(normalized)));
        }

        return query.ApplySorting(sortBy, sortDirection).ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public Task<PagedResult<Customers>> SearchAsync(string? name, string? phone, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Customers
            .AsNoTracking()
            .Include(x => x.IdUserNavigation)
            .AsQueryable();

        var normalizedName = string.IsNullOrWhiteSpace(name) ? null : name.Trim().ToLower();
        var normalizedPhone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();

        if (normalizedName is not null || normalizedPhone is not null)
        {
            query = query.Where(x =>
                (normalizedName != null && x.CustomerName.ToLower().Contains(normalizedName)) ||
                (normalizedPhone != null && x.PhoneNumber.Contains(normalizedPhone)));
        }

        return query.ApplySorting(sortBy, sortDirection).ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public Task<PagedResult<Customers>> GetByTypePagedAsync(string type, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Customers
            .AsNoTracking()
            .Include(x => x.IdUserNavigation)
            .Where(x => x.CustomerType != null && x.CustomerType == type.ToUpperInvariant());

        return query.ApplySorting(sortBy, sortDirection).ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public async Task AddAsync(Customers customer, CancellationToken cancellationToken = default)
    {
        await _dbContext.Customers.AddAsync(customer, cancellationToken);
    }

    public Task DeleteAsync(Customers customer, CancellationToken cancellationToken = default)
    {
        _dbContext.Customers.Remove(customer);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
