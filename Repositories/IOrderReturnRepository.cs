using StoreManagement.Common;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface IOrderReturnRepository
{
    Task<long> CountReturnOrdersByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<long> CountExchangeOrdersByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<OrderReturns?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderReturns>> GetMyReturnsAsync(int customerId, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderReturns>> SearchAsync(string? status, string? returnType, string? keyword, string? customerKeyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveReturnByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task AddAsync(OrderReturns entity, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}