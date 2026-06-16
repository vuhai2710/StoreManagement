using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class Products
{
    public int IdProduct { get; set; }

    public int? IdCategory { get; set; }

    /// <summary>
    /// Nhà cung cấp = Brand (Apple, Samsung...)
    /// </summary>
    public int? IdSupplier { get; set; }

    public string ProductName { get; set; } = null!;

    /// <summary>
    /// Thương hiệu
    /// </summary>
    public string? Brand { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? StockQuantity { get; set; }

    public string? Status { get; set; }

    public string? ImageUrl { get; set; }

    /// <summary>
    /// Mã: IMEI/Serial/SKU/Barcode
    /// </summary>
    public string ProductCode { get; set; } = null!;

    public string? CodeType { get; set; }

    /// <summary>
    /// SKU: PREFIX-XXXX
    /// </summary>
    public string? Sku { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDelete { get; set; }

    public virtual ICollection<CartItems> CartItems { get; set; } = new List<CartItems>();

    public virtual Categories? IdCategoryNavigation { get; set; }

    public virtual Suppliers? IdSupplierNavigation { get; set; }

    public virtual ICollection<InventoryTransactions> InventoryTransactions { get; set; } = new List<InventoryTransactions>();

    public virtual ICollection<OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();

    public virtual ICollection<OrderReturnItems> OrderReturnItems { get; set; } = new List<OrderReturnItems>();

    public virtual ICollection<ProductImages> ProductImages { get; set; } = new List<ProductImages>();

    public virtual ICollection<ProductPromotions> ProductPromotions { get; set; } = new List<ProductPromotions>();

    public virtual ICollection<ProductReviews> ProductReviews { get; set; } = new List<ProductReviews>();

    public virtual ICollection<PurchaseOrderDetails> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetails>();

    public virtual ICollection<Promotions> IdPromotion { get; set; } = new List<Promotions>();
}
