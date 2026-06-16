using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class Categories
{
    public int IdCategory { get; set; }

    public string CategoryName { get; set; } = null!;

    /// <summary>
    /// Prefix cho SKU: SP, LT, AO...
    /// </summary>
    public string? CodePrefix { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Products> Products { get; set; } = new List<Products>();
}
