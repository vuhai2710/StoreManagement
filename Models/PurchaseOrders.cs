using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class PurchaseOrders
{
    public int IdPurchaseOrder { get; set; }

    public int? IdSupplier { get; set; }

    public int? IdEmployee { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? InvoicePrinted { get; set; }

    public DateTime? InvoicePrintedAt { get; set; }

    public int? InvoicePrintedBy { get; set; }

    public virtual Employees? IdEmployeeNavigation { get; set; }

    public virtual Suppliers? IdSupplierNavigation { get; set; }

    public virtual ICollection<PurchaseOrderDetails> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetails>();
}
