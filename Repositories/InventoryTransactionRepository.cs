using Microsoft.EntityFrameworkCore;
using StoreManagement.Common;
using StoreManagement.Data;
using StoreManagement.Extensions;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class InventoryTransactionRepository : IInventoryTransactionRepository
{
    private readonly AppDbContext _dbContext;

    public InventoryTransactionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PagedResult<InventoryTransactions>> FilterAsync(
        string? transactionType,
        string? referenceType,
        int? referenceId,
        int? productId,
        string? productName,
        string? sku,
        string? brand,
        DateTime? fromDate,
        DateTime? toDate,
        int pageNo,
        int pageSize,
        string sortBy,
        string sortDirection,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.InventoryTransactions
            .AsNoTracking()
            .Include(x => x.IdProductNavigation)
            .Include(x => x.IdEmployeeNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(transactionType))
        {
            query = query.Where(x => x.TransactionType == transactionType);
        }

        if (!string.IsNullOrWhiteSpace(referenceType))
        {
            query = query.Where(x => x.ReferenceType == referenceType);
        }

        if (referenceId.HasValue)
        {
            query = query.Where(x => x.ReferenceId == referenceId.Value);
        }

        if (productId.HasValue)
        {
            query = query.Where(x => x.IdProduct == productId.Value);
        }

        if (!string.IsNullOrWhiteSpace(productName))
        {
            var normalized = productName.Trim().ToLower();
            query = query.Where(x => x.IdProductNavigation.ProductName.ToLower().Contains(normalized));
        }

        if (!string.IsNullOrWhiteSpace(sku))
        {
            var normalizedSku = sku.Trim().ToLower();
            query = query.Where(x => x.IdProductNavigation.Sku != null && x.IdProductNavigation.Sku.ToLower().Contains(normalizedSku));
        }

        if (!string.IsNullOrWhiteSpace(brand))
        {
            var normalizedBrand = brand.Trim().ToLower();
            query = query.Where(x => x.IdProductNavigation.Brand != null && x.IdProductNavigation.Brand.ToLower().Contains(normalizedBrand));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.TransactionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.TransactionDate <= toDate.Value);
        }

        return query.ApplySorting(sortBy, sortDirection).ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public Task AddAsync(InventoryTransactions entity, CancellationToken cancellationToken = default) =>
        _dbContext.InventoryTransactions.AddAsync(entity, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}