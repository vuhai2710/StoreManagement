using Microsoft.EntityFrameworkCore;
using StoreManagement.Common;
using StoreManagement.Data;
using StoreManagement.Extensions;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _dbContext;

    public OrderRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<long> CountByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Orders.LongCountAsync(x => x.IdEmployee == employeeId, cancellationToken);
    }

    public async Task<decimal> SumByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Where(x => x.IdEmployee == employeeId)
            .SumAsync(x => x.FinalAmount ?? x.TotalAmount ?? 0m, cancellationToken);
    }

    public Task<long> CountByEmployeeIdAndStatusAsync(int employeeId, string status, CancellationToken cancellationToken = default)
    {
        return _dbContext.Orders.LongCountAsync(x => x.IdEmployee == employeeId && x.Status == status, cancellationToken);
    }

    public Task<PagedResult<Orders>> GetByEmployeeIdAsync(int employeeId, string? status, DateTime? dateFrom, DateTime? dateTo, int pageNoZeroBased, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders
            .AsNoTracking()
            .Include(x => x.IdCustomerNavigation)
            .Where(x => x.IdEmployee == employeeId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(x => x.OrderDate >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(x => x.OrderDate <= dateTo.Value);
        }

        return query.OrderByDescending(x => x.OrderDate).ToPagedResultAsync(pageNoZeroBased + 1, pageSize, cancellationToken);
    }

    public async Task<List<(int ProductId, long TotalSold)>> GetBestSellingProductIdsAsync(string? status, int limit, int offset, CancellationToken cancellationToken = default)
    {
        var query =
            from detail in _dbContext.OrderDetails
            join order in _dbContext.Orders on detail.IdOrder equals order.IdOrder
            join product in _dbContext.Products on detail.IdProduct equals product.IdProduct
            where !product.IsDelete && (status == null || order.Status == status)
            group detail by detail.IdProduct into g
            orderby g.Sum(x => x.Quantity) descending
            select new { ProductId = g.Key, TotalSold = (long)g.Sum(x => x.Quantity) };

        return await query.Skip(offset).Take(limit).Select(x => ValueTuple.Create(x.ProductId, x.TotalSold)).ToListAsync(cancellationToken);
    }

    public async Task<long> CountBestSellingProductsAsync(string? status, CancellationToken cancellationToken = default)
    {
        var query =
            from detail in _dbContext.OrderDetails
            join order in _dbContext.Orders on detail.IdOrder equals order.IdOrder
            join product in _dbContext.Products on detail.IdProduct equals product.IdProduct
            where !product.IsDelete && (status == null || order.Status == status)
            group detail by detail.IdProduct into g
            select g.Key;

        return await query.LongCountAsync(cancellationToken);
    }

    public Task<Orders?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Orders
            .Include(x => x.IdCustomerNavigation)
            .Include(x => x.IdEmployeeNavigation)
            .Include(x => x.IdShippingAddressNavigation)
            .Include(x => x.IdPromotionNavigation)
            .Include(x => x.IdPromotionRuleNavigation)
            .Include(x => x.OrderDetails)
                .ThenInclude(x => x.IdProductNavigation)
            .Include(x => x.Shipments)
            .FirstOrDefaultAsync(x => x.IdOrder == id, cancellationToken);

    public Task<PagedResult<Orders>> SearchMyOrdersAsync(int customerId, string? status, string? keyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var query = BuildSearchQuery(status, customerId, keyword);
        query = query.Where(x => x.IdCustomer == customerId);
        query = query.ApplySorting(sortBy, sortDirection);
        return query.ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public Task<PagedResult<Orders>> SearchAllAsync(string? status, int? customerId, string? keyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var query = BuildSearchQuery(status, customerId, keyword).ApplySorting(sortBy, sortDirection);
        return query.ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public async Task<List<int>> FindPurchasedProductIdsByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await (
            from order in _dbContext.Orders
            join detail in _dbContext.OrderDetails on order.IdOrder equals detail.IdOrder
            where order.IdCustomer == customerId && order.Status == "COMPLETED"
            select detail.IdProduct
        ).Distinct().ToListAsync(cancellationToken);
    }

    public Task<Orders?> GetByPaymentLinkIdAsync(string paymentLinkId, CancellationToken cancellationToken = default) =>
        _dbContext.Orders
            .Include(x => x.OrderDetails)
                .ThenInclude(x => x.IdProductNavigation)
            .FirstOrDefaultAsync(x => x.PaymentLinkId == paymentLinkId, cancellationToken);

    public Task AddAsync(Orders order, CancellationToken cancellationToken = default) =>
        _dbContext.Orders.AddAsync(order, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<Orders> BuildSearchQuery(string? status, int? customerId, string? keyword)
    {
        var query = _dbContext.Orders
            .AsNoTracking()
            .Include(x => x.IdCustomerNavigation)
            .Include(x => x.IdEmployeeNavigation)
            .Include(x => x.OrderDetails)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (customerId.HasValue)
        {
            query = query.Where(x => x.IdCustomer == customerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim();
            query = query.Where(x =>
                x.IdOrder.ToString().Contains(term) ||
                (x.IdCustomerNavigation != null && x.IdCustomerNavigation.CustomerName != null && EF.Functions.Like(x.IdCustomerNavigation.CustomerName, $"%{term}%")) ||
                (x.IdCustomerNavigation != null && x.IdCustomerNavigation.PhoneNumber != null && EF.Functions.Like(x.IdCustomerNavigation.PhoneNumber, $"%{term}%")));
        }

        return query.OrderByDescending(x => x.OrderDate);
    }
}