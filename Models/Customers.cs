using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class Customers
{
    public int IdCustomer { get; set; }

    /// <summary>
    /// Liên kết tới bảng users
    /// </summary>
    public int? IdUser { get; set; }

    public string CustomerName { get; set; } = null!;

    public string? Address { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string? CustomerType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Carts? Carts { get; set; }

    public virtual ICollection<ChatConversations> ChatConversations { get; set; } = new List<ChatConversations>();

    public virtual Users? IdUserNavigation { get; set; }

    public virtual ICollection<OrderReturns> OrderReturns { get; set; } = new List<OrderReturns>();

    public virtual ICollection<Orders> Orders { get; set; } = new List<Orders>();

    public virtual ICollection<ProductReviews> ProductReviews { get; set; } = new List<ProductReviews>();

    public virtual ICollection<PromotionUsage> PromotionUsage { get; set; } = new List<PromotionUsage>();

    public virtual ICollection<ShippingAddresses> ShippingAddresses { get; set; } = new List<ShippingAddresses>();
}
