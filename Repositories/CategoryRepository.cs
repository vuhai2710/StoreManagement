using Microsoft.EntityFrameworkCore;
using StoreManagement.Common;
using StoreManagement.Data;
using StoreManagement.Extensions;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _dbContext;

    public CategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Categories?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Categories.FirstOrDefaultAsync(x => x.IdCategory == id, cancellationToken);
    }

    public Task<Categories?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return _dbContext.Categories.FirstOrDefaultAsync(x => x.CategoryName == name, cancellationToken);
    }

    public Task<List<Categories>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Categories.AsNoTracking().OrderBy(x => x.IdCategory).ToListAsync(cancellationToken);
    }

    public Task<PagedResult<Categories>> GetPagedAsync(int pageNo, int pageSize, string sortBy, string sortDirection, string? name, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Categories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => x.CategoryName.Contains(name));
        }

        return query
            .ApplySorting(sortBy, sortDirection)
            .ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public async Task AddAsync(Categories category, CancellationToken cancellationToken = default)
    {
        await _dbContext.Categories.AddAsync(category, cancellationToken);
    }

    public Task DeleteAsync(Categories category, CancellationToken cancellationToken = default)
    {
        _dbContext.Categories.Remove(category);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
