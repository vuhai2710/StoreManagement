using Microsoft.EntityFrameworkCore;
using StoreManagement.Common;
using StoreManagement.Data;
using StoreManagement.Extensions;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly AppDbContext _dbContext;

    public SupplierRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Suppliers?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Suppliers.FirstOrDefaultAsync(x => x.IdSupplier == id, cancellationToken);
    }

    public Task<Suppliers?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _dbContext.Suppliers.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<List<Suppliers>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Suppliers.AsNoTracking().OrderBy(x => x.IdSupplier).ToListAsync(cancellationToken);
    }

    public Task<PagedResult<Suppliers>> GetPagedAsync(int pageNo, int pageSize, string sortBy, string sortDirection, string? keyword, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Suppliers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.SupplierName.Contains(keyword));
        }

        return query
            .ApplySorting(sortBy, sortDirection)
            .ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public async Task AddAsync(Suppliers supplier, CancellationToken cancellationToken = default)
    {
        await _dbContext.Suppliers.AddAsync(supplier, cancellationToken);
    }

    public Task DeleteAsync(Suppliers supplier, CancellationToken cancellationToken = default)
    {
        _dbContext.Suppliers.Remove(supplier);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
