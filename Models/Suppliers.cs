using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class Suppliers
{
    public int IdSupplier { get; set; }

    public string SupplierName { get; set; } = null!;

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Products> Products { get; set; } = new List<Products>();

    public virtual ICollection<PurchaseOrders> PurchaseOrders { get; set; } = new List<PurchaseOrders>();
}
