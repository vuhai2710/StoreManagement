using Microsoft.EntityFrameworkCore;
using StoreManagement.Common;
using StoreManagement.Data;
using StoreManagement.Extensions;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class OrderReturnRepository : IOrderReturnRepository
{
    private readonly AppDbContext _dbContext;

    public OrderReturnRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<long> CountReturnOrdersByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return _dbContext.OrderReturns.LongCountAsync(x => x.ProcessedByEmployeeId == employeeId && x.ReturnType == "RETURN", cancellationToken);
    }

    public Task<long> CountExchangeOrdersByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return _dbContext.OrderReturns.LongCountAsync(x => x.ProcessedByEmployeeId == employeeId && x.ReturnType == "EXCHANGE", cancellationToken);
    }

    public Task<OrderReturns?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.OrderReturns
            .Include(x => x.IdOrderNavigation)
                .ThenInclude(x => x.OrderDetails)
                    .ThenInclude(x => x.IdProductNavigation)
            .Include(x => x.CreatedByCustomer)
            .Include(x => x.ProcessedByEmployee)
            .Include(x => x.OrderReturnItems)
                .ThenInclude(x => x.IdOrderDetailNavigation)
                    .ThenInclude(x => x.IdProductNavigation)
            .Include(x => x.OrderReturnItems)
                .ThenInclude(x => x.ExchangeProduct)
            .FirstOrDefaultAsync(x => x.IdReturn == id, cancellationToken);

    public Task<PagedResult<OrderReturns>> GetMyReturnsAsync(int customerId, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        return _dbContext.OrderReturns
            .AsNoTracking()
            .Include(x => x.IdOrderNavigation)
            .Include(x => x.OrderReturnItems)
            .Where(x => x.CreatedByCustomerId == customerId)
            .ApplySorting(sortBy, sortDirection)
            .ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public Task<PagedResult<OrderReturns>> SearchAsync(string? status, string? returnType, string? keyword, string? customerKeyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.OrderReturns
            .AsNoTracking()
            .Include(x => x.IdOrderNavigation)
            .Include(x => x.CreatedByCustomer)
            .Include(x => x.OrderReturnItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "ALL", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(returnType) && !string.Equals(returnType, "ALL", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.ReturnType == returnType);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim();
            query = query.Where(x => x.IdReturn.ToString().Contains(term) || x.IdOrder.ToString().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(customerKeyword))
        {
            var customerTerm = customerKeyword.Trim();
            query = query.Where(x =>
                x.CreatedByCustomer != null &&
                ((x.CreatedByCustomer.CustomerName != null && EF.Functions.Like(x.CreatedByCustomer.CustomerName, $"%{customerTerm}%")) ||
                 (x.CreatedByCustomer.PhoneNumber != null && EF.Functions.Like(x.CreatedByCustomer.PhoneNumber, $"%{customerTerm}%"))));
        }

        query = query.ApplySorting(sortBy, sortDirection);
        return query.ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public Task<bool> ExistsActiveReturnByOrderIdAsync(int orderId, CancellationToken cancellationToken = default) =>
        _dbContext.OrderReturns.AnyAsync(x => x.IdOrder == orderId && (x.Status == "REQUESTED" || x.Status == "APPROVED"), cancellationToken);

    public Task AddAsync(OrderReturns entity, CancellationToken cancellationToken = default) =>
        _dbContext.OrderReturns.AddAsync(entity, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}