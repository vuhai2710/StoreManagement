using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class Employees
{
    public int IdEmployee { get; set; }

    /// <summary>
    /// Liên kết tới bảng users
    /// </summary>
    public int? IdUser { get; set; }

    public string EmployeeName { get; set; } = null!;

    public DateOnly? HireDate { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public decimal? BaseSalary { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Users? IdUserNavigation { get; set; }

    public virtual ICollection<InventoryTransactions> InventoryTransactions { get; set; } = new List<InventoryTransactions>();

    public virtual ICollection<OrderReturns> OrderReturns { get; set; } = new List<OrderReturns>();

    public virtual ICollection<Orders> Orders { get; set; } = new List<Orders>();

    public virtual ICollection<PurchaseOrders> PurchaseOrders { get; set; } = new List<PurchaseOrders>();
}
