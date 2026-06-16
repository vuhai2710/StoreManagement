namespace StoreManagement.Dtos.Cart;

public class CartItemDto
{
    public int IdCartItem { get; set; }
    public int IdProduct { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductImageUrl { get; set; }
    public decimal ProductPrice { get; set; }
    public int ProductStockQuantity { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountedUnitPrice { get; set; }
    public decimal DiscountedSubtotal { get; set; }
    public decimal DiscountAmount { get; set; }
}
