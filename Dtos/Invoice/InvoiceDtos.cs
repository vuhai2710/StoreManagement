namespace StoreManagement.Dtos.Invoice;

public class InvoiceItemDto
{
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public int? Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? Subtotal { get; set; }
}

public class ExportInvoiceDto
{
    public int? OrderId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? ShippingAddressSnapshot { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? Status { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? Discount { get; set; }
    public decimal? ShippingFee { get; set; }
    public decimal? FinalAmount { get; set; }
    public bool? InvoicePrinted { get; set; }
    public DateTime? InvoicePrintedAt { get; set; }
    public int? InvoicePrintedBy { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = [];
}

public class ImportInvoiceDto
{
    public int? PurchaseOrderId { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierPhone { get; set; }
    public string? SupplierAddress { get; set; }
    public DateTime? OrderDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public bool? InvoicePrinted { get; set; }
    public DateTime? InvoicePrintedAt { get; set; }
    public int? InvoicePrintedBy { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = [];
}