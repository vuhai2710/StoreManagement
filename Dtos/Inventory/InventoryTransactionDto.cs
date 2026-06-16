namespace StoreManagement.Dtos.Inventory;

public class InventoryTransactionDto
{
    public int IdTransaction { get; set; }
    public int IdProduct { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public string? Sku { get; set; }
    public string? TransactionType { get; set; }
    public int Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public DateTime? TransactionDate { get; set; }
    public int? IdEmployee { get; set; }
    public string? EmployeeName { get; set; }
    public string? Notes { get; set; }
}
