using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Dtos.Cart;

public class UpdateCartItemRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Sá»‘ lÆ°á»£ng pháº£i lá»›n hÆ¡n 0")]
    public int Quantity { get; set; }
}
