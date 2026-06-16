using StoreManagement.Dtos.Shipment;
using StoreManagement.Exceptions;
using StoreManagement.Models;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public class ShippingAddressService : IShippingAddressService
{
    private readonly IShippingAddressRepository _shippingAddressRepository;
    private readonly ICustomerRepository _customerRepository;

    public ShippingAddressService(IShippingAddressRepository shippingAddressRepository, ICustomerRepository customerRepository)
    {
        _shippingAddressRepository = shippingAddressRepository;
        _customerRepository = customerRepository;
    }

    public async Task<List<ShippingAddressDto>> GetAllAddressesAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var addresses = await _shippingAddressRepository.GetAllByCustomerIdAsync(customerId, cancellationToken);
        return addresses.Select(MapToDto).ToList();
    }

    public async Task<ShippingAddressDto> GetDefaultAddressAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var address = await _shippingAddressRepository.GetDefaultByCustomerIdAsync(customerId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy địa chỉ mặc định");
        return MapToDto(address);
    }

    public async Task<ShippingAddressDto> CreateAddressAsync(int customerId, CreateShippingAddressRequestDto request, CancellationToken cancellationToken = default)
    {
        _ = await _customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy khách hàng");

        var address = new ShippingAddresses
        {
            IdCustomer = customerId,
            RecipientName = request.RecipientName,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            IsDefault = request.IsDefault ?? false,
            ProvinceId = request.ProvinceId,
            DistrictId = request.DistrictId,
            WardCode = request.WardCode
        };

        if (address.IsDefault == true)
        {
            var defaults = await _shippingAddressRepository.GetDefaultsByCustomerIdAsync(customerId, cancellationToken);
            defaults.ForEach(x => x.IsDefault = false);
        }

        await _shippingAddressRepository.AddAsync(address, cancellationToken);
        await _shippingAddressRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(address);
    }

    public async Task<ShippingAddressDto> UpdateAddressAsync(int customerId, int addressId, UpdateShippingAddressRequestDto request, CancellationToken cancellationToken = default)
    {
        var address = await _shippingAddressRepository.GetByIdAndCustomerIdAsync(addressId, customerId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy địa chỉ");

        address.RecipientName = request.RecipientName;
        address.PhoneNumber = request.PhoneNumber;
        address.Address = request.Address;
        address.ProvinceId = request.ProvinceId ?? address.ProvinceId;
        address.DistrictId = request.DistrictId ?? address.DistrictId;
        address.WardCode = request.WardCode ?? address.WardCode;

        await _shippingAddressRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(address);
    }

    public async Task<ShippingAddressDto> SetDefaultAddressAsync(int customerId, int addressId, CancellationToken cancellationToken = default)
    {
        var address = await _shippingAddressRepository.GetByIdAndCustomerIdAsync(addressId, customerId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy địa chỉ");

        var defaults = await _shippingAddressRepository.GetDefaultsByCustomerIdAsync(customerId, cancellationToken);
        defaults.Where(x => x.IdShippingAddress != addressId).ToList().ForEach(x => x.IsDefault = false);
        address.IsDefault = true;

        await _shippingAddressRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(address);
    }

    public async Task DeleteAddressAsync(int customerId, int addressId, CancellationToken cancellationToken = default)
    {
        var address = await _shippingAddressRepository.GetByIdAndCustomerIdAsync(addressId, customerId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy địa chỉ");

        if (address.IsDefault == true)
        {
            var defaults = await _shippingAddressRepository.GetDefaultsByCustomerIdAsync(customerId, cancellationToken);
            if (defaults.Count == 1)
            {
                throw new InvalidOperationException("Không thể xóa địa chỉ mặc định duy nhất");
            }
        }

        await _shippingAddressRepository.DeleteAsync(address, cancellationToken);
        await _shippingAddressRepository.SaveChangesAsync(cancellationToken);
    }

    private static ShippingAddressDto MapToDto(ShippingAddresses address)
    {
        return new ShippingAddressDto
        {
            IdShippingAddress = address.IdShippingAddress,
            IdCustomer = address.IdCustomer,
            RecipientName = address.RecipientName,
            PhoneNumber = address.PhoneNumber,
            Address = address.Address,
            IsDefault = address.IsDefault,
            ProvinceId = address.ProvinceId,
            DistrictId = address.DistrictId,
            WardCode = address.WardCode,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };
    }
}
