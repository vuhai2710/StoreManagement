using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Dtos.Cart;

public class AddToCartRequestDto
{
    [Required(ErrorMessage = "ID sản phẩm không được để trống")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
    public int Quantity { get; set; }
}
