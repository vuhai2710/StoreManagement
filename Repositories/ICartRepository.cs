using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface ICartRepository
{
    Task<Carts?> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task AddAsync(Carts cart, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
