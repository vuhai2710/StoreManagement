using Microsoft.EntityFrameworkCore;
using StoreManagement.Data;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _dbContext;

    public CartRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Carts?> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Carts
            .Include(x => x.CartItems)
                .ThenInclude(x => x.IdProductNavigation)
            .FirstOrDefaultAsync(x => x.IdCustomer == customerId, cancellationToken);
    }

    public async Task AddAsync(Carts cart, CancellationToken cancellationToken = default)
    {
        await _dbContext.Carts.AddAsync(cart, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
