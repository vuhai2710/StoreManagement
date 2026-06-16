using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

/// <summary>
/// Bảng lưu đánh giá sản phẩm của khách hàng
/// </summary>
public partial class ProductReviews
{
    public int IdReview { get; set; }

    public int IdProduct { get; set; }

    public int IdCustomer { get; set; }

    /// <summary>
    /// Reference đến order đã mua sản phẩm
    /// </summary>
    public int? IdOrder { get; set; }

    /// <summary>
    /// Reference đến order detail cụ thể
    /// </summary>
    public int? IdOrderDetail { get; set; }

    public int Rating { get; set; }

    public string Comment { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Câu trả lời từ admin/employee
    /// </summary>
    public string? AdminReply { get; set; }

    /// <summary>
    /// Số lần đã chỉnh sửa review (tối đa 1 lần)
    /// </summary>
    public int EditCount { get; set; }

    public virtual Customers IdCustomerNavigation { get; set; } = null!;

    public virtual OrderDetails? IdOrderDetailNavigation { get; set; }

    public virtual Orders? IdOrderNavigation { get; set; }

    public virtual Products IdProductNavigation { get; set; } = null!;
}
