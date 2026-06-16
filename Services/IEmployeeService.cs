using StoreManagement.Common;
using StoreManagement.Dtos.Employee;

namespace StoreManagement.Services;

public interface IEmployeeService
{
    Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto request, CancellationToken cancellationToken = default);
    Task<List<EmployeeDto>> GetAllEmployeesAsync(CancellationToken cancellationToken = default);
    Task<PageResponse<EmployeeDto>> GetAllEmployeesPaginatedAsync(string? keyword, int pageNoZeroBased, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<EmployeeDto> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeDetailDto> GetEmployeeDetailByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PageResponse<EmployeeOrderDto>> GetOrdersByEmployeeIdAsync(int id, string? status, DateTime? dateFrom, DateTime? dateTo, int pageNoZeroBased, int pageSize, CancellationToken cancellationToken = default);
    Task<EmployeeDto> GetEmployeeByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<EmployeeDto> UpdateEmployeeByAdminAsync(int id, EmployeeDto request, CancellationToken cancellationToken = default);
    Task DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeDto> GetMyProfileAsync(string username, CancellationToken cancellationToken = default);
    Task<EmployeeDto> UpdateMyProfileAsync(string username, EmployeeDto request, CancellationToken cancellationToken = default);
}
