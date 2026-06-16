using StoreManagement.Common;
using StoreManagement.Dtos.Inventory;
using StoreManagement.Extensions;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public class InventoryTransactionService : IInventoryTransactionService
{
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;

    public InventoryTransactionService(IInventoryTransactionRepository inventoryTransactionRepository)
    {
        _inventoryTransactionRepository = inventoryTransactionRepository;
    }

    public async Task<PageResponse<InventoryTransactionDto>> FilterTransactionsAsync(
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
        var page = await _inventoryTransactionRepository.FilterAsync(transactionType, referenceType, referenceId, productId, productName, sku, brand, fromDate, toDate, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<InventoryTransactionDto>
        {
            Items = page.Items.Select(x => new InventoryTransactionDto
            {
                IdTransaction = x.IdTransaction,
                IdProduct = x.IdProduct,
                ProductName = x.IdProductNavigation.ProductName,
                ProductCode = x.IdProductNavigation.ProductCode,
                Sku = x.IdProductNavigation.Sku,
                TransactionType = x.TransactionType,
                Quantity = x.Quantity,
                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,
                TransactionDate = x.TransactionDate,
                IdEmployee = x.IdEmployee,
                EmployeeName = x.IdEmployeeNavigation?.EmployeeName,
                Notes = x.Notes
            }).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }
}
