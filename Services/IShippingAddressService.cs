using StoreManagement.Dtos.Shipment;

namespace StoreManagement.Services;

public interface IShippingAddressService
{
    Task<List<ShippingAddressDto>> GetAllAddressesAsync(int customerId, CancellationToken cancellationToken = default);
    Task<ShippingAddressDto> GetDefaultAddressAsync(int customerId, CancellationToken cancellationToken = default);
    Task<ShippingAddressDto> CreateAddressAsync(int customerId, CreateShippingAddressRequestDto request, CancellationToken cancellationToken = default);
    Task<ShippingAddressDto> UpdateAddressAsync(int customerId, int addressId, UpdateShippingAddressRequestDto request, CancellationToken cancellationToken = default);
    Task<ShippingAddressDto> SetDefaultAddressAsync(int customerId, int addressId, CancellationToken cancellationToken = default);
    Task DeleteAddressAsync(int customerId, int addressId, CancellationToken cancellationToken = default);
}
