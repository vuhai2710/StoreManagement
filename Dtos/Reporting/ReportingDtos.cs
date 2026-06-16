namespace StoreManagement.Dtos.Reporting;

public class DashboardOverviewDto
{
    public decimal? TodayRevenue { get; set; }
    public long? OrdersToday { get; set; }
    public long? CompletedOrdersToday { get; set; }
    public decimal? MonthRevenue { get; set; }
    public long? OrdersThisMonth { get; set; }
    public long? CompletedOrdersThisMonth { get; set; }
    public long? ActiveReturnRequests { get; set; }
    public List<TopProductDto> TopProducts { get; set; } = [];
    public List<RecentOrderDto> RecentOrders { get; set; } = [];
    public List<DailyRevenueDto> RevenueChart { get; set; } = [];
}

public class TopProductDto
{
    public int? ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public string? ImageUrl { get; set; }
    public long? QuantitySold { get; set; }
    public decimal? NetRevenue { get; set; }
}

public class RecentOrderDto
{
    public int? OrderId { get; set; }
    public string? CustomerName { get; set; }
    public string? OrderDate { get; set; }
    public string? Status { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? FinalAmount { get; set; }
}

public class DailyRevenueDto
{
    public string? Date { get; set; }
    public decimal? NetRevenue { get; set; }
    public long? OrderCount { get; set; }
}

public class RevenueSummaryDto
{
    public decimal? ProductRevenue { get; set; }
    public decimal? TotalDiscount { get; set; }
    public decimal? NetRevenue { get; set; }
    public decimal? ImportCost { get; set; }
    public decimal? GrossProfit { get; set; }
    public long? TotalOrders { get; set; }
    public long? CompletedOrders { get; set; }
    public long? CanceledOrders { get; set; }
    public long? PendingOrders { get; set; }
    public long? ConfirmedOrders { get; set; }
    public long? ReturnedOrders { get; set; }
    public decimal? RefundAmount { get; set; }
    public decimal? ShippingFeeTotal { get; set; }
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
    public string? GroupBy { get; set; }
}

public class RevenueByTimeDto
{
    public string? Time { get; set; }
    public decimal? ProductRevenue { get; set; }
    public decimal? TotalDiscount { get; set; }
    public decimal? NetRevenue { get; set; }
    public long? OrderCount { get; set; }
}

public class RevenueByProductDto
{
    public int? ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public string? CategoryName { get; set; }
    public long? QuantitySold { get; set; }
    public decimal? ProductRevenue { get; set; }
    public decimal? Discount { get; set; }
    public decimal? NetRevenue { get; set; }
    public decimal? AvgSellingPrice { get; set; }
}