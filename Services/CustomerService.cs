using StoreManagement.Common;
using StoreManagement.Dtos.Customer;
using StoreManagement.Exceptions;
using StoreManagement.Extensions;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUserRepository _userRepository;

    public CustomerService(ICustomerRepository customerRepository, IUserRepository userRepository)
    {
        _customerRepository = customerRepository;
        _userRepository = userRepository;
    }

    public async Task<PageResponse<CustomerDto>> GetAllCustomersPaginatedAsync(string? keyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var page = await _customerRepository.GetPagedAsync(keyword, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<CustomerDto>
        {
            Items = page.Items.Select(MapToDto).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<CustomerDto> GetCustomerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Customer không tồn tại với ID: {id}");
        return MapToDto(customer);
    }

    public async Task<PageResponse<CustomerDto>> SearchCustomersAsync(string? name, string? phone, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var page = await _customerRepository.SearchAsync(name, phone, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<CustomerDto>
        {
            Items = page.Items.Select(MapToDto).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<PageResponse<CustomerDto>> GetCustomersByTypeAsync(string type, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var normalized = type.ToUpperInvariant();
        if (normalized is not ("VIP" or "REGULAR"))
        {
            throw new InvalidOperationException($"Customer type không hợp lệ: {type}");
        }

        var page = await _customerRepository.GetByTypePagedAsync(normalized, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<CustomerDto>
        {
            Items = page.Items.Select(MapToDto).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<CustomerDto> UpdateCustomerAsync(int id, CustomerDto customerDto, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Customer không tồn tại với ID: {id}");

        if (!string.IsNullOrWhiteSpace(customerDto.Email))
        {
            throw new InvalidOperationException("Email không được phép cập nhật. Email chỉ được set khi tạo tài khoản.");
        }

        if (!string.IsNullOrWhiteSpace(customerDto.PhoneNumber))
        {
            var existing = await _customerRepository.GetByPhoneNumberAsync(customerDto.PhoneNumber, cancellationToken);
            if (existing is not null && existing.IdCustomer != id)
            {
                throw new ConflictException("Số điện thoại đã được sử dụng");
            }

            customer.PhoneNumber = customerDto.PhoneNumber;
        }

        if (!string.IsNullOrWhiteSpace(customerDto.CustomerName))
        {
            customer.CustomerName = customerDto.CustomerName;
        }

        if (customerDto.Address is not null)
        {
            customer.Address = customerDto.Address;
        }

        if (!string.IsNullOrWhiteSpace(customerDto.CustomerType))
        {
            customer.CustomerType = customerDto.CustomerType.ToUpperInvariant();
        }

        await _customerRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(customer);
    }

    public async Task<CustomerDto> UpgradeToVipAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Customer không tồn tại với ID: {id}");
        customer.CustomerType = "VIP";
        await _customerRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(customer);
    }

    public async Task<CustomerDto> DowngradeToRegularAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Customer không tồn tại với ID: {id}");
        customer.CustomerType = "REGULAR";
        await _customerRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(customer);
    }

    public async Task DeleteCustomerAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Customer không tồn tại với ID: {id}");

        if (customer.IdUser.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(customer.IdUser.Value, cancellationToken);
            await _customerRepository.DeleteAsync(customer, cancellationToken);
            await _customerRepository.SaveChangesAsync(cancellationToken);
            if (user is not null)
            {
                await _userRepository.DeleteAsync(user, cancellationToken);
                await _userRepository.SaveChangesAsync(cancellationToken);
            }
            return;
        }

        await _customerRepository.DeleteAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<CustomerDto> GetCustomerByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByUsernameAsync(username, cancellationToken)
            ?? throw new ResourceNotFoundException($"Customer không tồn tại cho user: {username}");
        return MapToDto(customer);
    }

    public async Task<CustomerDto> UpdateMyCustomerInfoAsync(string username, CustomerDto customerDto, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByUsernameAsync(username, cancellationToken)
            ?? throw new ResourceNotFoundException($"Customer không tồn tại cho user: {username}");
        return await UpdateCustomerAsync(customer.IdCustomer, customerDto, cancellationToken);
    }

    private static CustomerDto MapToDto(StoreManagement.Models.Customers customer)
    {
        return new CustomerDto
        {
            IdCustomer = customer.IdCustomer,
            IdUser = customer.IdUser,
            Username = customer.IdUserNavigation?.Username,
            Email = customer.IdUserNavigation?.Email,
            CustomerName = customer.CustomerName,
            PhoneNumber = customer.PhoneNumber,
            Address = customer.Address,
            CustomerType = customer.CustomerType,
            IsActive = customer.IdUserNavigation?.IsActive,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}
