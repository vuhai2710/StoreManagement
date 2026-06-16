using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class ProductImages
{
    public int IdProductImage { get; set; }

    public int IdProduct { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool? IsPrimary { get; set; }

    public int? DisplayOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Products IdProductNavigation { get; set; } = null!;
}
