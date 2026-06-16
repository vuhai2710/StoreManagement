using StoreManagement.Common;
using StoreManagement.Dtos.Inventory;

namespace StoreManagement.Services;

public interface IInventoryTransactionService
{
    Task<PageResponse<InventoryTransactionDto>> FilterTransactionsAsync(
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
}
