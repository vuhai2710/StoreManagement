using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class OrderDetails
{
    public int IdOrderDetail { get; set; }

    public int IdOrder { get; set; }

    public int IdProduct { get; set; }

    public int Quantity { get; set; }

    /// <summary>
    /// Giá tại thời điểm mua
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Tên sản phẩm tại thời điểm mua
    /// </summary>
    public string? ProductNameSnapshot { get; set; }

    /// <summary>
    /// Mã sản phẩm tại thời điểm mua
    /// </summary>
    public string? ProductCodeSnapshot { get; set; }

    /// <summary>
    /// URL ảnh tại thời điểm mua
    /// </summary>
    public string? ProductImageSnapshot { get; set; }

    public virtual Orders IdOrderNavigation { get; set; } = null!;

    public virtual Products IdProductNavigation { get; set; } = null!;

    public virtual ICollection<OrderReturnItems> OrderReturnItems { get; set; } = new List<OrderReturnItems>();

    public virtual ProductReviews? ProductReviews { get; set; }
}
