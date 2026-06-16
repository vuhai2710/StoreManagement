using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

/// <summary>
/// Bảng lưu lịch sử sử dụng mã giảm giá
/// </summary>
public partial class PromotionUsage
{
    public int IdUsage { get; set; }

    public int IdPromotion { get; set; }

    public int IdOrder { get; set; }

    public int IdCustomer { get; set; }

    public DateTime? UsedAt { get; set; }

    public virtual Customers IdCustomerNavigation { get; set; } = null!;

    public virtual Orders IdOrderNavigation { get; set; } = null!;

    public virtual Promotions IdPromotionNavigation { get; set; } = null!;
}
