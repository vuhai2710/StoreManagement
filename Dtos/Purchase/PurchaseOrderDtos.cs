namespace StoreManagement.Dtos.Purchase;

public class PurchaseOrderDto
{
    public int? IdImportOrder { get; set; }
    public int? IdSupplier { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierAddress { get; set; }
    public string? SupplierPhone { get; set; }
    public string? SupplierEmail { get; set; }
    public int? IdEmployee { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime? OrderDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public List<PurchaseOrderDetailDto>? ImportOrderDetails { get; set; }
}

public class PurchaseOrderDetailDto
{
    public int? IdImportOrderDetail { get; set; }
    public int? IdProduct { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public string? Sku { get; set; }
    public int? Quantity { get; set; }
    public decimal? ImportPrice { get; set; }
    public decimal? Subtotal { get; set; }
}