using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Inventory;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/inventory-transactions")]
public class InventoryTransactionController : ControllerBase
{
    private readonly IInventoryTransactionService _inventoryTransactionService;

    public InventoryTransactionController(IInventoryTransactionService inventoryTransactionService)
    {
        _inventoryTransactionService = inventoryTransactionService;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public Task<ActionResult<ApiResponse<PageResponse<InventoryTransactionDto>>>> GetAllTransactions(
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "TransactionDate",
        [FromQuery] string sortDirection = "DESC",
        CancellationToken cancellationToken = default)
        => FilterInternal(null, null, null, null, null, null, null, null, null, pageNo, pageSize, sortBy, sortDirection, "Lấy lịch sử nhập/xuất kho thành công", cancellationToken);

    [HttpGet("product/{productId:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public Task<ActionResult<ApiResponse<PageResponse<InventoryTransactionDto>>>> GetTransactionsByProduct(
        int productId,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "TransactionDate",
        [FromQuery] string sortDirection = "DESC",
        CancellationToken cancellationToken = default)
        => FilterInternal(null, null, null, productId, null, null, null, null, null, pageNo, pageSize, sortBy, sortDirection, "Lấy lịch sử nhập/xuất kho của sản phẩm thành công", cancellationToken);

    [HttpGet("reference")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public Task<ActionResult<ApiResponse<PageResponse<InventoryTransactionDto>>>> GetTransactionsByReference(
        [FromQuery] string referenceType,
        [FromQuery] int referenceId,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "TransactionDate",
        [FromQuery] string sortDirection = "DESC",
        CancellationToken cancellationToken = default)
        => FilterInternal(null, referenceType, referenceId, null, null, null, null, null, null, pageNo, pageSize, sortBy, sortDirection, "Lấy lịch sử nhập/xuất kho theo reference thành công", cancellationToken);

    [HttpGet("history")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public Task<ActionResult<ApiResponse<PageResponse<InventoryTransactionDto>>>> GetHistory(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? productId = null,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "TransactionDate",
        [FromQuery] string sortDirection = "DESC",
        CancellationToken cancellationToken = default)
        => FilterInternal(null, null, null, productId, null, null, null, startDate ?? DateTime.UtcNow.AddMonths(-1), endDate ?? DateTime.UtcNow, pageNo, pageSize, sortBy, sortDirection, "Lấy lịch sử nhập/xuất kho thành công", cancellationToken);

    [HttpGet("by-type")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public Task<ActionResult<ApiResponse<PageResponse<InventoryTransactionDto>>>> GetByType(
        [FromQuery] string transactionType,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "TransactionDate",
        [FromQuery] string sortDirection = "DESC",
        CancellationToken cancellationToken = default)
        => FilterInternal(transactionType, null, null, null, null, null, null, null, null, pageNo, pageSize, sortBy, sortDirection, "Lấy lịch sử theo loại giao dịch thành công", cancellationToken);

    [HttpGet("filter")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public Task<ActionResult<ApiResponse<PageResponse<InventoryTransactionDto>>>> Filter(
        [FromQuery] string? transactionType = null,
        [FromQuery] int? productId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "TransactionDate",
        [FromQuery] string sortDirection = "DESC",
        CancellationToken cancellationToken = default)
        => FilterInternal(transactionType, null, null, productId, null, null, null, startDate ?? DateTime.UtcNow.AddMonths(-1), endDate ?? DateTime.UtcNow, pageNo, pageSize, sortBy, sortDirection, "Lọc lịch sử nhập/xuất kho thành công", cancellationToken);

    [HttpGet("filter-advanced")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public Task<ActionResult<ApiResponse<PageResponse<InventoryTransactionDto>>>> FilterAdvanced(
        [FromQuery] string? transactionType = null,
        [FromQuery] string? referenceType = null,
        [FromQuery] int? productId = null,
        [FromQuery] string? productName = null,
        [FromQuery] string? sku = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "TransactionDate",
        [FromQuery] string sortDirection = "DESC",
        CancellationToken cancellationToken = default)
        => FilterInternal(transactionType, referenceType, null, productId, productName, sku, null, startDate, endDate, pageNo, pageSize, sortBy, sortDirection, "Lọc lịch sử nhập/xuất kho thành công", cancellationToken);

    [HttpGet("search")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public Task<ActionResult<ApiResponse<PageResponse<InventoryTransactionDto>>>> Search(
        [FromQuery] string? transactionType = null,
        [FromQuery] string? referenceType = null,
        [FromQuery] int? referenceId = null,
        [FromQuery] int? productId = null,
        [FromQuery] string? productName = null,
        [FromQuery] string? sku = null,
        [FromQuery] string? brand = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "TransactionDate",
        [FromQuery] string sortDirection = "DESC",
        CancellationToken cancellationToken = default)
        => FilterInternal(transactionType, referenceType, referenceId, productId, productName, sku, brand, fromDate, toDate, pageNo, pageSize, sortBy, sortDirection, "Tìm kiếm lịch sử nhập/xuất kho thành công", cancellationToken);

    private async Task<ActionResult<ApiResponse<PageResponse<InventoryTransactionDto>>>> FilterInternal(
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
        string message,
        CancellationToken cancellationToken)
    {
        var page = await _inventoryTransactionService.FilterTransactionsAsync(transactionType, referenceType, referenceId, productId, productName, sku, brand, fromDate, toDate, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<InventoryTransactionDto>>.Success(message, page));
    }
}
