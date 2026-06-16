using StoreManagement.Dtos.Base;

namespace StoreManagement.Dtos.Employee;

public class EmployeeDetailDto : BaseDto
{
    public int? IdEmployee { get; set; }
    public int? IdUser { get; set; }
    public bool? IsActive { get; set; }
    public string? EmployeeName { get; set; }
    public DateOnly? HireDate { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public decimal? BaseSalary { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public long TotalOrdersHandled { get; set; }
    public decimal TotalOrderAmount { get; set; }
    public long TotalReturnOrders { get; set; }
    public long TotalExchangeOrders { get; set; }
    public long PendingOrders { get; set; }
    public long CompletedOrders { get; set; }
    public long CancelledOrders { get; set; }
}
