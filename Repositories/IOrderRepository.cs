using StoreManagement.Common;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface IOrderRepository
{
    Task<long> CountByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<decimal> SumByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<long> CountByEmployeeIdAndStatusAsync(int employeeId, string status, CancellationToken cancellationToken = default);
    Task<PagedResult<Orders>> GetByEmployeeIdAsync(int employeeId, string? status, DateTime? dateFrom, DateTime? dateTo, int pageNoZeroBased, int pageSize, CancellationToken cancellationToken = default);
    Task<List<(int ProductId, long TotalSold)>> GetBestSellingProductIdsAsync(string? status, int limit, int offset, CancellationToken cancellationToken = default);
    Task<long> CountBestSellingProductsAsync(string? status, CancellationToken cancellationToken = default);
    Task<Orders?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<Orders>> SearchMyOrdersAsync(int customerId, string? status, string? keyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<PagedResult<Orders>> SearchAllAsync(string? status, int? customerId, string? keyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<List<int>> FindPurchasedProductIdsByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<Orders?> GetByPaymentLinkIdAsync(string paymentLinkId, CancellationToken cancellationToken = default);
    Task AddAsync(Orders order, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}