using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface IProductImageRepository
{
    Task<List<ProductImages>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<long> CountByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<ProductImages?> GetByIdAsync(int imageId, CancellationToken cancellationToken = default);
    Task<ProductImages?> GetPrimaryAsync(int productId, CancellationToken cancellationToken = default);
    Task<ProductImages?> GetFirstAsync(int productId, CancellationToken cancellationToken = default);
    Task AddAsync(ProductImages image, CancellationToken cancellationToken = default);
    Task DeleteAsync(ProductImages image, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
