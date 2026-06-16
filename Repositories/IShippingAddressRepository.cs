using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface IShippingAddressRepository
{
    Task<List<ShippingAddresses>> GetAllByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<ShippingAddresses?> GetDefaultByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<ShippingAddresses?> GetByIdAndCustomerIdAsync(int addressId, int customerId, CancellationToken cancellationToken = default);
    Task<List<ShippingAddresses>> GetDefaultsByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task AddAsync(ShippingAddresses address, CancellationToken cancellationToken = default);
    Task DeleteAsync(ShippingAddresses address, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
