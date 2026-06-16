using StoreManagement.Dtos.Cart;

namespace StoreManagement.Services;

public interface ICartService
{
    Task<CartDto> GetCartAsync(int customerId, CancellationToken cancellationToken = default);
    Task<CartDto> AddToCartAsync(int customerId, AddToCartRequestDto request, CancellationToken cancellationToken = default);
    Task<CartDto> UpdateCartItemAsync(int customerId, int itemId, UpdateCartItemRequestDto request, CancellationToken cancellationToken = default);
    Task<CartDto> RemoveCartItemAsync(int customerId, int itemId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(int customerId, CancellationToken cancellationToken = default);
}
