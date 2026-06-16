using Microsoft.EntityFrameworkCore;
using StoreManagement.Data;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class ProductImageRepository : IProductImageRepository
{
    private readonly AppDbContext _dbContext;

    public ProductImageRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<ProductImages>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ProductImages.Where(x => x.IdProduct == productId).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);
    }

    public Task<long> CountByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ProductImages.LongCountAsync(x => x.IdProduct == productId, cancellationToken);
    }

    public Task<ProductImages?> GetByIdAsync(int imageId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ProductImages.Include(x => x.IdProductNavigation).FirstOrDefaultAsync(x => x.IdProductImage == imageId, cancellationToken);
    }

    public Task<ProductImages?> GetPrimaryAsync(int productId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ProductImages.FirstOrDefaultAsync(x => x.IdProduct == productId && x.IsPrimary == true, cancellationToken);
    }

    public Task<ProductImages?> GetFirstAsync(int productId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ProductImages.Where(x => x.IdProduct == productId).OrderBy(x => x.DisplayOrder).ThenBy(x => x.IdProductImage).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(ProductImages image, CancellationToken cancellationToken = default)
    {
        await _dbContext.ProductImages.AddAsync(image, cancellationToken);
    }

    public Task DeleteAsync(ProductImages image, CancellationToken cancellationToken = default)
    {
        _dbContext.ProductImages.Remove(image);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
