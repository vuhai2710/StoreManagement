using Microsoft.EntityFrameworkCore;
using StoreManagement.Data;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class CartItemRepository : ICartItemRepository
{
    private readonly AppDbContext _dbContext;

    public CartItemRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CartItems?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.CartItems
            .Include(x => x.IdCartNavigation)
            .Include(x => x.IdProductNavigation)
            .FirstOrDefaultAsync(x => x.IdCartItem == id, cancellationToken);
    }

    public Task<CartItems?> GetByCartAndProductAsync(int cartId, int productId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CartItems.FirstOrDefaultAsync(x => x.IdCart == cartId && x.IdProduct == productId, cancellationToken);
    }

    public async Task AddAsync(CartItems item, CancellationToken cancellationToken = default)
    {
        await _dbContext.CartItems.AddAsync(item, cancellationToken);
    }

    public Task DeleteAsync(CartItems item, CancellationToken cancellationToken = default)
    {
        _dbContext.CartItems.Remove(item);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
