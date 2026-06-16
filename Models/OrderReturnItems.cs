using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class OrderReturnItems
{
    public int IdReturnItem { get; set; }

    public int IdReturn { get; set; }

    public int IdOrderDetail { get; set; }

    public int Quantity { get; set; }

    public int? ExchangeProductId { get; set; }

    public int? ExchangeQuantity { get; set; }

    public decimal? LineRefundAmount { get; set; }

    public virtual Products? ExchangeProduct { get; set; }

    public virtual OrderDetails IdOrderDetailNavigation { get; set; } = null!;

    public virtual OrderReturns IdReturnNavigation { get; set; } = null!;
}
