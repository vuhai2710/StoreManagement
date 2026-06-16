using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

/// <summary>
/// Bảng lưu quy tắc giảm giá tự động
/// </summary>
public partial class PromotionRules
{
    public int IdRule { get; set; }

    public string RuleName { get; set; } = null!;

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public string? CustomerType { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool? IsActive { get; set; }

    /// <summary>
    /// Ưu tiên (số càng cao càng ưu tiên)
    /// </summary>
    public int? Priority { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Phạm vi áp dụng: ORDER = giảm giá đơn hàng, SHIPPING = giảm giá phí vận chuyển, PRODUCT = giảm giá sản phẩm
    /// </summary>
    public string Scope { get; set; } = null!;

    public virtual ICollection<Orders> Orders { get; set; } = new List<Orders>();
}
