using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class InventoryTransactions
{
    public int IdTransaction { get; set; }

    public int IdProduct { get; set; }

    public string TransactionType { get; set; } = null!;

    public int Quantity { get; set; }

    public string? ReferenceType { get; set; }

    /// <summary>
    /// ID của purchase_orders / orders (tuỳ loại)
    /// </summary>
    public int? ReferenceId { get; set; }

    public DateTime? TransactionDate { get; set; }

    public int? IdEmployee { get; set; }

    public string? Notes { get; set; }

    public virtual Employees? IdEmployeeNavigation { get; set; }

    public virtual Products IdProductNavigation { get; set; } = null!;
}
