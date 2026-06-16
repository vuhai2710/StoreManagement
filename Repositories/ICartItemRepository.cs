using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface ICartItemRepository
{
    Task<CartItems?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CartItems?> GetByCartAndProductAsync(int cartId, int productId, CancellationToken cancellationToken = default);
    Task AddAsync(CartItems item, CancellationToken cancellationToken = default);
    Task DeleteAsync(CartItems item, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
