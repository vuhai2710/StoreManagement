using BCrypt.Net;
using StoreManagement.Common;
using StoreManagement.Dtos.Employee;
using StoreManagement.Exceptions;
using StoreManagement.Extensions;
using StoreManagement.Models;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderReturnRepository _orderReturnRepository;

    public EmployeeService(IEmployeeRepository employeeRepository, IUserRepository userRepository, IOrderRepository orderRepository, IOrderReturnRepository orderReturnRepository)
    {
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _orderReturnRepository = orderReturnRepository;
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new InvalidOperationException("Email không được để trống");
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new InvalidOperationException("Tên đăng nhập không được để trống");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Mật khẩu không được để trống");
        }

        if (await _userRepository.GetByUsernameAsync(request.Username, cancellationToken) is not null)
        {
            throw new ConflictException($"Username đã tồn tại: {request.Username}");
        }

        if (await _userRepository.GetByEmailAsync(request.Email, cancellationToken) is not null)
        {
            throw new ConflictException($"Email đã tồn tại: {request.Email}");
        }

        if (await _employeeRepository.ExistsByPhoneNumberAsync(request.PhoneNumber, null, cancellationToken))
        {
            throw new ConflictException($"Số điện thoại đã tồn tại: {request.PhoneNumber}");
        }

        var user = new Users
        {
            Username = request.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
            Email = request.Email,
            Role = "EMPLOYEE",
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var employee = new Employees
        {
            IdUser = user.IdUser,
            EmployeeName = request.EmployeeName,
            HireDate = request.HireDate,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            BaseSalary = request.BaseSalary
        };

        await _employeeRepository.AddAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        employee = await _employeeRepository.GetByIdAsync(employee.IdEmployee, cancellationToken) ?? employee;
        return MapToDto(employee);
    }

    public async Task<List<EmployeeDto>> GetAllEmployeesAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        return employees.Select(MapToDto).ToList();
    }

    public async Task<PageResponse<EmployeeDto>> GetAllEmployeesPaginatedAsync(string? keyword, int pageNoZeroBased, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var page = await _employeeRepository.GetPagedAsync(keyword, pageNoZeroBased + 1, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<EmployeeDto>
        {
            Items = page.Items.Select(MapToDto).ToList(),
            PageNo = page.PageNo - 1,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<EmployeeDto> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Nhân viên không tồn tại với ID: {id}");
        return MapToDto(employee);
    }

    public async Task<EmployeeDetailDto> GetEmployeeDetailByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Nhân viên không tồn tại với ID: {id}");

        return new EmployeeDetailDto
        {
            IdEmployee = employee.IdEmployee,
            IdUser = employee.IdUser,
            IsActive = employee.IdUserNavigation?.IsActive,
            EmployeeName = employee.EmployeeName,
            HireDate = employee.HireDate,
            PhoneNumber = employee.PhoneNumber,
            Address = employee.Address,
            BaseSalary = employee.BaseSalary,
            Username = employee.IdUserNavigation?.Username,
            Email = employee.IdUserNavigation?.Email,
            AvatarUrl = employee.IdUserNavigation?.AvatarUrl,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt,
            TotalOrdersHandled = await _orderRepository.CountByEmployeeIdAsync(id, cancellationToken),
            TotalOrderAmount = await _orderRepository.SumByEmployeeIdAsync(id, cancellationToken),
            PendingOrders = await _orderRepository.CountByEmployeeIdAndStatusAsync(id, "PENDING", cancellationToken),
            CompletedOrders = await _orderRepository.CountByEmployeeIdAndStatusAsync(id, "COMPLETED", cancellationToken),
            CancelledOrders = await _orderRepository.CountByEmployeeIdAndStatusAsync(id, "CANCELED", cancellationToken),
            TotalReturnOrders = await _orderReturnRepository.CountReturnOrdersByEmployeeIdAsync(id, cancellationToken),
            TotalExchangeOrders = await _orderReturnRepository.CountExchangeOrdersByEmployeeIdAsync(id, cancellationToken)
        };
    }

    public async Task<PageResponse<EmployeeOrderDto>> GetOrdersByEmployeeIdAsync(int id, string? status, DateTime? dateFrom, DateTime? dateTo, int pageNoZeroBased, int pageSize, CancellationToken cancellationToken = default)
    {
        _ = await _employeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Nhân viên không tồn tại với ID: {id}");

        var page = await _orderRepository.GetByEmployeeIdAsync(id, status, dateFrom, dateTo, pageNoZeroBased, pageSize, cancellationToken);
        return new PagedResult<EmployeeOrderDto>
        {
            Items = page.Items.Select(order => new EmployeeOrderDto
            {
                IdOrder = order.IdOrder,
                CustomerName = order.IdCustomerNavigation?.CustomerName,
                TotalAmount = order.TotalAmount,
                Discount = order.Discount,
                FinalAmount = order.FinalAmount,
                Status = order.Status,
                CreatedAt = order.OrderDate
            }).ToList(),
            PageNo = page.PageNo - 1,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<EmployeeDto> GetEmployeeByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Nhân viên không tồn tại với User ID: {userId}");
        return MapToDto(employee);
    }

    public async Task<EmployeeDto> UpdateEmployeeByAdminAsync(int id, EmployeeDto request, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Nhân viên không tồn tại với ID: {id}");

        var user = employee.IdUserNavigation ?? throw new ResourceNotFoundException("Không tìm thấy user của nhân viên");

        if (!string.IsNullOrWhiteSpace(request.Username) && !string.Equals(request.Username, user.Username, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
            if (existingUser is not null && existingUser.IdUser != user.IdUser)
            {
                throw new ConflictException($"Username đã được sử dụng: {request.Username}");
            }

            user.Username = request.Username;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            throw new InvalidOperationException("Email không được phép cập nhật. Email chỉ được set khi tạo tài khoản.");
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && !string.Equals(request.PhoneNumber, employee.PhoneNumber, StringComparison.OrdinalIgnoreCase))
        {
            if (await _employeeRepository.ExistsByPhoneNumberAsync(request.PhoneNumber, id, cancellationToken))
            {
                throw new ConflictException($"Số điện thoại đã tồn tại: {request.PhoneNumber}");
            }

            employee.PhoneNumber = request.PhoneNumber;
        }

        if (!string.IsNullOrWhiteSpace(request.EmployeeName))
        {
            employee.EmployeeName = request.EmployeeName;
        }

        if (request.HireDate.HasValue)
        {
            employee.HireDate = request.HireDate;
        }

        if (request.Address is not null)
        {
            employee.Address = request.Address;
        }

        if (request.BaseSalary.HasValue)
        {
            employee.BaseSalary = request.BaseSalary.Value;
        }

        await _employeeRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(employee);
    }

    public async Task DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Nhân viên không tồn tại với ID: {id}");

        var user = employee.IdUserNavigation;
        await _employeeRepository.DeleteAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        if (user is not null)
        {
            await _userRepository.DeleteAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<EmployeeDto> GetMyProfileAsync(string username, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByUsernameAsync(username, cancellationToken)
            ?? throw new ResourceNotFoundException($"Thông tin nhân viên không tồn tại cho user: {username}");
        return MapToDto(employee);
    }

    public async Task<EmployeeDto> UpdateMyProfileAsync(string username, EmployeeDto request, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByUsernameAsync(username, cancellationToken)
            ?? throw new ResourceNotFoundException($"Thông tin nhân viên không tồn tại cho user: {username}");

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            throw new InvalidOperationException("Email không được phép cập nhật. Email chỉ được set khi tạo tài khoản.");
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && !string.Equals(request.PhoneNumber, employee.PhoneNumber, StringComparison.OrdinalIgnoreCase))
        {
            if (await _employeeRepository.ExistsByPhoneNumberAsync(request.PhoneNumber, employee.IdEmployee, cancellationToken))
            {
                throw new ConflictException($"Số điện thoại đã được sử dụng: {request.PhoneNumber}");
            }

            employee.PhoneNumber = request.PhoneNumber;
        }

        if (!string.IsNullOrWhiteSpace(request.EmployeeName))
        {
            employee.EmployeeName = request.EmployeeName;
        }

        if (request.HireDate.HasValue)
        {
            employee.HireDate = request.HireDate;
        }

        if (request.Address is not null)
        {
            employee.Address = request.Address;
        }

        await _employeeRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(employee);
    }

    private static EmployeeDto MapToDto(Employees employee)
    {
        return new EmployeeDto
        {
            IdEmployee = employee.IdEmployee,
            IdUser = employee.IdUser,
            IsActive = employee.IdUserNavigation?.IsActive,
            EmployeeName = employee.EmployeeName,
            HireDate = employee.HireDate,
            PhoneNumber = employee.PhoneNumber ?? string.Empty,
            Address = employee.Address,
            BaseSalary = employee.BaseSalary,
            Username = employee.IdUserNavigation?.Username,
            Email = employee.IdUserNavigation?.Email,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt
        };
    }
}
