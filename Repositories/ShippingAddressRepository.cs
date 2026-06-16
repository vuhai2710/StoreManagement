using Microsoft.EntityFrameworkCore;
using StoreManagement.Data;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class ShippingAddressRepository : IShippingAddressRepository
{
    private readonly AppDbContext _dbContext;

    public ShippingAddressRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<ShippingAddresses>> GetAllByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ShippingAddresses
            .AsNoTracking()
            .Where(x => x.IdCustomer == customerId)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.IdShippingAddress)
            .ToListAsync(cancellationToken);
    }

    public Task<ShippingAddresses?> GetDefaultByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ShippingAddresses.FirstOrDefaultAsync(x => x.IdCustomer == customerId && x.IsDefault == true, cancellationToken);
    }

    public Task<ShippingAddresses?> GetByIdAndCustomerIdAsync(int addressId, int customerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ShippingAddresses.FirstOrDefaultAsync(x => x.IdShippingAddress == addressId && x.IdCustomer == customerId, cancellationToken);
    }

    public Task<List<ShippingAddresses>> GetDefaultsByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ShippingAddresses.Where(x => x.IdCustomer == customerId && x.IsDefault == true).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ShippingAddresses address, CancellationToken cancellationToken = default)
    {
        await _dbContext.ShippingAddresses.AddAsync(address, cancellationToken);
    }

    public Task DeleteAsync(ShippingAddresses address, CancellationToken cancellationToken = default)
    {
        _dbContext.ShippingAddresses.Remove(address);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
