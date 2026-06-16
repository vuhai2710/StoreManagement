using StoreManagement.Common;
using StoreManagement.Dtos.Customer;

namespace StoreManagement.Services;

public interface ICustomerService
{
    Task<PageResponse<CustomerDto>> GetAllCustomersPaginatedAsync(string? keyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<CustomerDto> GetCustomerByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PageResponse<CustomerDto>> SearchCustomersAsync(string? name, string? phone, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<PageResponse<CustomerDto>> GetCustomersByTypeAsync(string type, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateCustomerAsync(int id, CustomerDto customerDto, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpgradeToVipAsync(int id, CancellationToken cancellationToken = default);
    Task<CustomerDto> DowngradeToRegularAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteCustomerAsync(int id, CancellationToken cancellationToken = default);
    Task<CustomerDto> GetCustomerByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateMyCustomerInfoAsync(string username, CustomerDto customerDto, CancellationToken cancellationToken = default);
}
