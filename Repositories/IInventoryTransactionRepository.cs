using StoreManagement.Common;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface IInventoryTransactionRepository
{
    Task<PagedResult<InventoryTransactions>> FilterAsync(
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
        CancellationToken cancellationToken = default);
    Task AddAsync(InventoryTransactions entity, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}