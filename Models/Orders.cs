using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class Orders
{
    public int IdOrder { get; set; }

    public int? IdCustomer { get; set; }

    public int? IdEmployee { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? Status { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? Discount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Reference đến shipping_addresses
    /// </summary>
    public int? IdShippingAddress { get; set; }

    /// <summary>
    /// Snapshot của địa chỉ tại thời điểm đặt hàng
    /// </summary>
    public string? ShippingAddressSnapshot { get; set; }

    /// <summary>
    /// Thời điểm customer xác nhận đã nhận hàng
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// PayOS payment link ID - được set khi tạo payment link thành công
    /// </summary>
    public string? PaymentLinkId { get; set; }

    /// <summary>
    /// Reference đến promotions (nếu sử dụng mã giảm giá)
    /// </summary>
    public int? IdPromotion { get; set; }

    /// <summary>
    /// Mã giảm giá được sử dụng
    /// </summary>
    public string? PromotionCode { get; set; }

    /// <summary>
    /// Reference đến promotion_rules (nếu áp dụng discount tự động)
    /// </summary>
    public int? IdPromotionRule { get; set; }

    /// <summary>
    /// Snapshot số ngày cho phép đổi/trả tại thời điểm hoàn thành đơn
    /// </summary>
    public int? ReturnWindowDays { get; set; }

    /// <summary>
    /// Phí giao hàng
    /// </summary>
    public decimal? ShippingFee { get; set; }

    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Tổng tiền cuối = total_amount + shipping_fee - discount (&gt;=0)
    /// </summary>
    public decimal? FinalAmount { get; set; }

    /// <summary>
    /// Giảm giá phí vận chuyển riêng biệt với giảm giá đơn hàng
    /// </summary>
    public decimal? ShippingDiscount { get; set; }

    /// <summary>
    /// Mã giảm giá phí vận chuyển đã sử dụng
    /// </summary>
    public string? ShippingPromotionCode { get; set; }

    public int? IdShippingPromotion { get; set; }

    public bool? InvoicePrinted { get; set; }

    public DateTime? InvoicePrintedAt { get; set; }

    public int? InvoicePrintedBy { get; set; }

    public virtual Customers? IdCustomerNavigation { get; set; }

    public virtual Employees? IdEmployeeNavigation { get; set; }

    public virtual Promotions? IdPromotionNavigation { get; set; }

    public virtual PromotionRules? IdPromotionRuleNavigation { get; set; }

    public virtual ShippingAddresses? IdShippingAddressNavigation { get; set; }

    public virtual Promotions? IdShippingPromotionNavigation { get; set; }

    public virtual ICollection<OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();

    public virtual ICollection<OrderReturns> OrderReturns { get; set; } = new List<OrderReturns>();

    public virtual ICollection<ProductReviews> ProductReviews { get; set; } = new List<ProductReviews>();

    public virtual ICollection<PromotionUsage> PromotionUsage { get; set; } = new List<PromotionUsage>();

    public virtual Shipments? Shipments { get; set; }
}
