using Microsoft.EntityFrameworkCore;
using StoreManagement.Common;
using StoreManagement.Data;
using StoreManagement.Extensions;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Users?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.IdUser == id, cancellationToken);
    }

    public Task<Users?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
    }

    public Task<Users?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<PagedResult<Users>> GetPagedAsync(int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .ApplySorting(sortBy, sortDirection)
            .ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public Task<PagedResult<Users>> GetByStatusPagedAsync(bool isActive, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .Where(x => (x.IsActive ?? false) == isActive)
            .ApplySorting(sortBy, sortDirection)
            .ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public async Task AddAsync(Users user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public Task DeleteAsync(Users user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Remove(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
