using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class PurchaseOrderDetails
{
    public int IdPurchaseOrderDetail { get; set; }

    public int IdPurchaseOrder { get; set; }

    public int IdProduct { get; set; }

    public int Quantity { get; set; }

    public decimal ImportPrice { get; set; }

    public virtual Products IdProductNavigation { get; set; } = null!;

    public virtual PurchaseOrders IdPurchaseOrderNavigation { get; set; } = null!;
}
