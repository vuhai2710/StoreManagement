using Microsoft.EntityFrameworkCore;
using StoreManagement.Common;
using StoreManagement.Data;
using StoreManagement.Dtos.Reporting;

namespace StoreManagement.Services;

public interface IDashboardService
{
    Task<DashboardOverviewDto> GetDashboardOverviewAsync(int topProducts, int recentOrders, int chartDays, CancellationToken cancellationToken = default);
}

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _dbContext;

    public DashboardService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardOverviewDto> GetDashboardOverviewAsync(int topProducts, int recentOrders, int chartDays, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var completedOrdersToday = await _dbContext.Orders.Where(x => x.OrderDate >= today && x.Status == "COMPLETED").ToListAsync(cancellationToken);
        var completedOrdersMonth = await _dbContext.Orders.Where(x => x.OrderDate >= monthStart && x.Status == "COMPLETED").ToListAsync(cancellationToken);
        var allOrdersToday = await _dbContext.Orders.LongCountAsync(x => x.OrderDate >= today, cancellationToken);
        var allOrdersMonth = await _dbContext.Orders.LongCountAsync(x => x.OrderDate >= monthStart, cancellationToken);
        var activeReturns = await _dbContext.OrderReturns.LongCountAsync(x => x.Status == "REQUESTED", cancellationToken);

        var topProductsData = await (
            from order in _dbContext.Orders
            join detail in _dbContext.OrderDetails on order.IdOrder equals detail.IdOrder
            join product in _dbContext.Products on detail.IdProduct equals product.IdProduct
            where order.Status == "COMPLETED" && order.OrderDate >= monthStart
            group new { detail, product } by new { product.IdProduct, product.ProductCode, product.ProductName, product.ImageUrl } into g
            orderby g.Sum(x => x.detail.Quantity) descending
            select new TopProductDto
            {
                ProductId = g.Key.IdProduct,
                ProductCode = g.Key.ProductCode,
                ProductName = g.Key.ProductName,
                ImageUrl = g.Key.ImageUrl,
                QuantitySold = g.Sum(x => (long)x.detail.Quantity),
                NetRevenue = g.Sum(x => x.detail.Price * x.detail.Quantity)
            }).Take(topProducts).ToListAsync(cancellationToken);

        var recentOrdersData = await _dbContext.Orders
            .OrderByDescending(x => x.OrderDate)
            .Include(x => x.IdCustomerNavigation)
            .Take(recentOrders)
            .Select(x => new RecentOrderDto
            {
                OrderId = x.IdOrder,
                CustomerName = x.IdCustomerNavigation != null ? x.IdCustomerNavigation.CustomerName : null,
                OrderDate = x.OrderDate.HasValue ? x.OrderDate.Value.ToString("dd/MM/yyyy HH:mm") : null,
                Status = x.Status,
                TotalAmount = x.TotalAmount,
                FinalAmount = x.FinalAmount
            }).ToListAsync(cancellationToken);

        var startChartDate = today.AddDays(-(chartDays - 1));
        var chart = new List<DailyRevenueDto>();
        for (var day = startChartDate; day <= today; day = day.AddDays(1))
        {
            var dayEnd = day.AddDays(1);
            var orders = await _dbContext.Orders.Where(x => x.Status == "COMPLETED" && x.OrderDate >= day && x.OrderDate < dayEnd).ToListAsync(cancellationToken);
            chart.Add(new DailyRevenueDto
            {
                Date = day.ToString("dd/MM"),
                NetRevenue = orders.Sum(x => (x.FinalAmount ?? x.TotalAmount) ?? 0m),
                OrderCount = orders.LongCount()
            });
        }

        return new DashboardOverviewDto
        {
            TodayRevenue = completedOrdersToday.Sum(x => (x.FinalAmount ?? x.TotalAmount) ?? 0m),
            OrdersToday = allOrdersToday,
            CompletedOrdersToday = completedOrdersToday.LongCount(),
            MonthRevenue = completedOrdersMonth.Sum(x => (x.FinalAmount ?? x.TotalAmount) ?? 0m),
            OrdersThisMonth = allOrdersMonth,
            CompletedOrdersThisMonth = completedOrdersMonth.LongCount(),
            ActiveReturnRequests = activeReturns,
            TopProducts = topProductsData,
            RecentOrders = recentOrdersData,
            RevenueChart = chart
        };
    }
}

public interface IReportService
{
    Task<RevenueSummaryDto> GetRevenueSummaryAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<List<RevenueByTimeDto>> GetRevenueByTimeAsync(DateTime fromDate, DateTime toDate, string groupBy, CancellationToken cancellationToken = default);
    Task<List<RevenueByProductDto>> GetRevenueByProductAsync(DateTime fromDate, DateTime toDate, int limit, CancellationToken cancellationToken = default);
}

public class ReportService : IReportService
{
    private readonly AppDbContext _dbContext;

    public ReportService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RevenueSummaryDto> GetRevenueSummaryAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var orders = await _dbContext.Orders
            .Where(x => x.OrderDate >= fromDate && x.OrderDate <= toDate)
            .ToListAsync(cancellationToken);
        var completed = orders.Where(x => x.Status == "COMPLETED").ToList();
        var approvedReturns = await _dbContext.OrderReturns
            .Where(x => x.Status == "APPROVED" && x.CreatedAt >= fromDate && x.CreatedAt <= toDate)
            .ToListAsync(cancellationToken);

        var importCost = await (
            from purchaseDetail in _dbContext.PurchaseOrderDetails
            join purchaseOrder in _dbContext.PurchaseOrders on purchaseDetail.IdPurchaseOrder equals purchaseOrder.IdPurchaseOrder
            where purchaseOrder.OrderDate >= fromDate && purchaseOrder.OrderDate <= toDate
            select purchaseDetail.ImportPrice * purchaseDetail.Quantity
        ).DefaultIfEmpty(0m).SumAsync(cancellationToken);

        var productRevenue = completed.Sum(x => x.TotalAmount ?? 0m);
        var totalDiscount = completed.Sum(x => x.Discount ?? 0m);
        var netRevenue = Math.Max(0m, completed.Sum(x => (x.FinalAmount ?? x.TotalAmount) ?? 0m));

        return new RevenueSummaryDto
        {
            ProductRevenue = productRevenue,
            TotalDiscount = totalDiscount,
            NetRevenue = netRevenue,
            ImportCost = importCost,
            GrossProfit = netRevenue - importCost,
            TotalOrders = orders.LongCount(),
            CompletedOrders = orders.LongCount(x => x.Status == "COMPLETED"),
            CanceledOrders = orders.LongCount(x => x.Status == "CANCELED"),
            PendingOrders = orders.LongCount(x => x.Status == "PENDING"),
            ConfirmedOrders = orders.LongCount(x => x.Status == "CONFIRMED"),
            ReturnedOrders = approvedReturns.LongCount(),
            RefundAmount = approvedReturns.Sum(x => x.RefundAmount ?? 0m),
            ShippingFeeTotal = completed.Sum(x => x.ShippingFee ?? 0m),
            FromDate = fromDate.ToString("yyyy-MM-dd"),
            ToDate = toDate.ToString("yyyy-MM-dd")
        };
    }

    public async Task<List<RevenueByTimeDto>> GetRevenueByTimeAsync(DateTime fromDate, DateTime toDate, string groupBy, CancellationToken cancellationToken = default)
    {
        var orders = await _dbContext.Orders
            .Where(x => x.Status == "COMPLETED" && x.OrderDate >= fromDate && x.OrderDate <= toDate)
            .ToListAsync(cancellationToken);

        var groups = groupBy.ToUpperInvariant() switch
        {
            "DAY" => orders.GroupBy(x => x.OrderDate?.ToString("yyyy-MM-dd")),
            "YEAR" => orders.GroupBy(x => x.OrderDate?.ToString("yyyy")),
            _ => orders.GroupBy(x => x.OrderDate?.ToString("yyyy-MM"))
        };

        return groups
            .OrderBy(x => x.Key)
            .Select(g => new RevenueByTimeDto
            {
                Time = g.Key,
                ProductRevenue = g.Sum(x => x.TotalAmount ?? 0m),
                TotalDiscount = g.Sum(x => x.Discount ?? 0m),
                NetRevenue = g.Sum(x => (x.FinalAmount ?? x.TotalAmount) ?? 0m),
                OrderCount = g.LongCount()
            })
            .ToList();
    }

    public async Task<List<RevenueByProductDto>> GetRevenueByProductAsync(DateTime fromDate, DateTime toDate, int limit, CancellationToken cancellationToken = default)
    {
        var query = (
            from order in _dbContext.Orders
            join detail in _dbContext.OrderDetails on order.IdOrder equals detail.IdOrder
            join product in _dbContext.Products on detail.IdProduct equals product.IdProduct
            join category in _dbContext.Categories on product.IdCategory equals category.IdCategory into categoryJoin
            from category in categoryJoin.DefaultIfEmpty()
            where order.Status == "COMPLETED" && order.OrderDate >= fromDate && order.OrderDate <= toDate
            group new { detail, product, category } by new { product.IdProduct, product.ProductCode, product.ProductName, CategoryName = category != null ? category.CategoryName : null } into g
            orderby g.Sum(x => x.detail.Price * x.detail.Quantity) descending
            select new RevenueByProductDto
            {
                ProductId = g.Key.IdProduct,
                ProductCode = g.Key.ProductCode,
                ProductName = g.Key.ProductName,
                CategoryName = g.Key.CategoryName,
                QuantitySold = g.Sum(x => (long)x.detail.Quantity),
                ProductRevenue = g.Sum(x => x.detail.Price * x.detail.Quantity),
                Discount = 0m,
                NetRevenue = g.Sum(x => x.detail.Price * x.detail.Quantity),
                AvgSellingPrice = g.Sum(x => x.detail.Price * x.detail.Quantity) / Math.Max(1, g.Sum(x => x.detail.Quantity))
            }).Take(limit > 0 ? limit : 20);

        return await query.ToListAsync(cancellationToken);
    }
}