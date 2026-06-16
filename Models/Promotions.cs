using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

/// <summary>
/// Bảng lưu mã giảm giá
/// </summary>
public partial class Promotions
{
    public int IdPromotion { get; set; }

    public string Code { get; set; } = null!;

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public decimal? MinOrderAmount { get; set; }

    /// <summary>
    /// Số lần sử dụng tối đa (NULL = không giới hạn)
    /// </summary>
    public int? UsageLimit { get; set; }

    public int? UsageCount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Phạm vi áp dụng: ORDER = giảm giá đơn hàng, SHIPPING = giảm giá phí vận chuyển, PRODUCT = giảm giá sản phẩm
    /// </summary>
    public string Scope { get; set; } = null!;

    public virtual ICollection<Orders> OrdersIdPromotionNavigation { get; set; } = new List<Orders>();

    public virtual ICollection<Orders> OrdersIdShippingPromotionNavigation { get; set; } = new List<Orders>();

    public virtual ICollection<PromotionUsage> PromotionUsage { get; set; } = new List<PromotionUsage>();

    public virtual ICollection<Products> IdProduct { get; set; } = new List<Products>();
}
