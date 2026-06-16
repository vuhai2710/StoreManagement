namespace StoreManagement.Dtos.Order;

public class OrderDto
{
    public int? IdOrder { get; set; }
    public int? IdCustomer { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerPhone { get; set; }
    public int? IdEmployee { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? Status { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? Discount { get; set; }
    public decimal? FinalAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public int? IdShippingAddress { get; set; }
    public string? ShippingAddressSnapshot { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? ReturnWindowDays { get; set; }
    public decimal? ShippingFee { get; set; }
    public string? PaymentLinkId { get; set; }
    public string? PaymentLinkUrl { get; set; }
    public List<OrderDetailDto>? OrderDetails { get; set; }
    public int? ShippingAddressId { get; set; }
    public string? PromotionCode { get; set; }
    public int? IdPromotion { get; set; }
    public int? IdPromotionRule { get; set; }
    public string? PromotionName { get; set; }
    public string? PromotionDiscountType { get; set; }
    public decimal? PromotionDiscountValue { get; set; }
    public string? PromotionScope { get; set; }
    public decimal? ShippingDiscount { get; set; }
    public string? ShippingPromotionCode { get; set; }
    public int? IdShippingPromotion { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerNameForCreate { get; set; }
    public string? CustomerPhoneForCreate { get; set; }
    public string? CustomerAddressForCreate { get; set; }
    public List<OrderDetailDto>? OrderItems { get; set; }
    public int? ProductId { get; set; }
    public int? Quantity { get; set; }
}

public class OrderDetailDto
{
    public int? IdOrderDetail { get; set; }
    public int? IdProduct { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public string? Sku { get; set; }
    public string? ProductNameSnapshot { get; set; }
    public string? ProductCodeSnapshot { get; set; }
    public string? ProductImageSnapshot { get; set; }
    public int? Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? Subtotal { get; set; }
    public int? ProductId { get; set; }
    public int? QuantityForCreate { get; set; }
}

public class OrderReturnDto
{
    public int? IdReturn { get; set; }
    public int? OrderId { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? ReturnType { get; set; }
    public string? Status { get; set; }
    public string? Reason { get; set; }
    public string? NoteAdmin { get; set; }
    public decimal? RefundAmount { get; set; }
    public decimal? OrderFinalAmount { get; set; }
    public decimal? OrderTotalAmount { get; set; }
    public decimal? OrderDiscount { get; set; }
    public decimal? OrderShippingFee { get; set; }
    public string? OrderPromotionCode { get; set; }
    public string? OrderPromotionName { get; set; }
    public string? OrderPromotionDiscountType { get; set; }
    public decimal? OrderPromotionDiscountValue { get; set; }
    public string? OrderPromotionScope { get; set; }
    public int? CreatedByCustomerId { get; set; }
    public int? ProcessedByEmployeeId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderReturnItemDto>? Items { get; set; }
}

public class OrderReturnItemDto
{
    public int? IdReturnItem { get; set; }
    public int? IdOrderDetail { get; set; }
    public int? Quantity { get; set; }
    public string? ProductName { get; set; }
    public decimal? Price { get; set; }
    public int? ExchangeProductId { get; set; }
    public int? ExchangeQuantity { get; set; }
    public decimal? LineRefundAmount { get; set; }
}