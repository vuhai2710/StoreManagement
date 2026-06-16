using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class ProductPromotions
{
    public int IdProductPromotion { get; set; }

    public int IdProduct { get; set; }

    /// <summary>
    /// PERCENTAGE or FIXED_AMOUNT
    /// </summary>
    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }

    public bool? IsActive { get; set; }

    public string? PromotionName { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Products IdProductNavigation { get; set; } = null!;
}
