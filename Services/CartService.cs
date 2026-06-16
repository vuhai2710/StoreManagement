using StoreManagement.Dtos.Cart;
using StoreManagement.Exceptions;
using StoreManagement.Models;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;

    public CartService(ICartRepository cartRepository, ICartItemRepository cartItemRepository, ICustomerRepository customerRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
    }

    public async Task<CartDto> GetCartAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(customerId, cancellationToken);
        var validItems = cart.CartItems.Where(x => !x.IdProductNavigation.IsDelete).ToList();
        return MapToDto(cart, validItems);
    }

    public async Task<CartDto> AddToCartAsync(int customerId, AddToCartRequestDto request, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(customerId, cancellationToken);
        var product = await _productRepository.GetByIdAsync(request.ProductId, false, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy sản phẩm");

        if (product.Status == "DISCONTINUED" || product.IsDelete)
        {
            throw new InvalidOperationException("Sản phẩm đã ngừng kinh doanh hoặc không còn tồn tại");
        }

        var availableStock = product.StockQuantity ?? 0;
        if (product.Status == "OUT_OF_STOCK")
        {
            if (availableStock > 0)
            {
                product.Status = "IN_STOCK";
                await _productRepository.SaveChangesAsync(cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("Sản phẩm đã hết hàng");
            }
        }

        if (availableStock < request.Quantity)
        {
            throw new InvalidOperationException($"Số lượng sản phẩm không đủ. Còn lại: {availableStock}");
        }

        var existingItem = await _cartItemRepository.GetByCartAndProductAsync(cart.IdCart, request.ProductId, cancellationToken);
        if (existingItem is not null)
        {
            var newQuantity = existingItem.Quantity + request.Quantity;
            if (availableStock < newQuantity)
            {
                throw new InvalidOperationException($"Số lượng sản phẩm không đủ. Còn lại: {availableStock}");
            }

            existingItem.Quantity = newQuantity;
            await _cartItemRepository.SaveChangesAsync(cancellationToken);
        }
        else
        {
            await _cartItemRepository.AddAsync(new CartItems
            {
                IdCart = cart.IdCart,
                IdProduct = request.ProductId,
                Quantity = request.Quantity
            }, cancellationToken);
            await _cartItemRepository.SaveChangesAsync(cancellationToken);
        }

        return await GetCartAsync(customerId, cancellationToken);
    }

    public async Task<CartDto> UpdateCartItemAsync(int customerId, int itemId, UpdateCartItemRequestDto request, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(customerId, cancellationToken);
        var item = await _cartItemRepository.GetByIdAsync(itemId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy sản phẩm trong giỏ hàng");

        if (item.IdCart != cart.IdCart)
        {
            throw new InvalidOperationException("Không có quyền cập nhật sản phẩm này");
        }

        var availableStock = item.IdProductNavigation.StockQuantity ?? 0;
        if (availableStock < request.Quantity)
        {
            throw new InvalidOperationException($"Số lượng sản phẩm không đủ. Còn lại: {availableStock}");
        }

        item.Quantity = request.Quantity;
        await _cartItemRepository.SaveChangesAsync(cancellationToken);
        return await GetCartAsync(customerId, cancellationToken);
    }

    public async Task<CartDto> RemoveCartItemAsync(int customerId, int itemId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(customerId, cancellationToken);
        var item = await _cartItemRepository.GetByIdAsync(itemId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy sản phẩm trong giỏ hàng");

        if (item.IdCart != cart.IdCart)
        {
            throw new InvalidOperationException("Không có quyền xóa sản phẩm này");
        }

        await _cartItemRepository.DeleteAsync(item, cancellationToken);
        await _cartItemRepository.SaveChangesAsync(cancellationToken);
        return await GetCartAsync(customerId, cancellationToken);
    }

    public async Task ClearCartAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateCartAsync(customerId, cancellationToken);
        foreach (var item in cart.CartItems.ToList())
        {
            await _cartItemRepository.DeleteAsync(item, cancellationToken);
        }

        await _cartItemRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Carts> GetOrCreateCartAsync(int customerId, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        if (cart is not null)
        {
            return cart;
        }

        _ = await _customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy khách hàng");

        cart = new Carts
        {
            IdCustomer = customerId
        };

        await _cartRepository.AddAsync(cart, cancellationToken);
        await _cartRepository.SaveChangesAsync(cancellationToken);
        return await _cartRepository.GetByCustomerIdAsync(customerId, cancellationToken) ?? cart;
    }

    private static CartDto MapToDto(Carts cart, List<CartItems> items)
    {
        var cartItems = items.Select(item =>
        {
            var price = item.IdProductNavigation.Price;
            var subtotal = price * item.Quantity;
            return new CartItemDto
            {
                IdCartItem = item.IdCartItem,
                IdProduct = item.IdProduct,
                ProductName = item.IdProductNavigation.ProductName,
                ProductCode = item.IdProductNavigation.ProductCode,
                ProductImageUrl = item.IdProductNavigation.ImageUrl,
                ProductPrice = price,
                ProductStockQuantity = item.IdProductNavigation.StockQuantity ?? 0,
                Quantity = item.Quantity,
                Subtotal = subtotal,
                DiscountAmount = 0,
                DiscountedSubtotal = subtotal,
                DiscountedUnitPrice = price
            };
        }).ToList();

        var total = cartItems.Sum(x => x.Subtotal);
        return new CartDto
        {
            IdCart = cart.IdCart,
            IdCustomer = cart.IdCustomer,
            CartItems = cartItems,
            TotalAmount = total,
            TotalItems = cartItems.Sum(x => x.Quantity),
            AutomaticDiscount = 0,
            FinalAmount = total,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };
    }
}
