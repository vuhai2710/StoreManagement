using StoreManagement.Dtos.Base;

namespace StoreManagement.Dtos.Cart;

public class CartDto : BaseDto
{
    public int IdCart { get; set; }
    public int IdCustomer { get; set; }
    public List<CartItemDto> CartItems { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
    public decimal AutomaticDiscount { get; set; }
    public decimal FinalAmount { get; set; }
}
