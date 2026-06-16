using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class OrderReturns
{
    public int IdReturn { get; set; }

    public int IdOrder { get; set; }

    public string ReturnType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? Reason { get; set; }

    public string? NoteAdmin { get; set; }

    public decimal? RefundAmount { get; set; }

    public int? CreatedByCustomerId { get; set; }

    public int? ProcessedByEmployeeId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Customers? CreatedByCustomer { get; set; }

    public virtual Orders IdOrderNavigation { get; set; } = null!;

    public virtual ICollection<OrderReturnItems> OrderReturnItems { get; set; } = new List<OrderReturnItems>();

    public virtual Employees? ProcessedByEmployee { get; set; }
}
