namespace StoreManagement.Dtos.Employee;

public class EmployeeOrderDto
{
    public int IdOrder { get; set; }
    public string? CustomerName { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? Discount { get; set; }
    public decimal? FinalAmount { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
}
