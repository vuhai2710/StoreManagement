using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Employee;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/employees")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> CreateEmployee([FromBody] EmployeeDto request, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.CreateEmployeeAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<EmployeeDto>.Success("Tạo nhân viên thành công", employee));
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<List<EmployeeDto>>>> GetAllEmployees(CancellationToken cancellationToken)
    {
        var employees = await _employeeService.GetAllEmployeesAsync(cancellationToken);
        return Ok(ApiResponse<List<EmployeeDto>>.Success("Lấy danh sách nhân viên thành công", employees));
    }

    [HttpGet("paginated")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<PageResponse<EmployeeDto>>>> GetAllEmployeesPaginated(
        [FromQuery] int pageNo = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "IdEmployee",
        [FromQuery] string sortDirection = "DESC",
        [FromQuery] string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        var page = await _employeeService.GetAllEmployeesPaginatedAsync(keyword, Math.Max(0, pageNo), Math.Max(1, Math.Min(100, pageSize)), sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<EmployeeDto>>.Success("Lấy danh sách nhân viên thành công", page));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetEmployeeById(int id, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<EmployeeDto>.Success("Lấy thông tin nhân viên thành công", employee));
    }

    [HttpGet("{id:int}/detail")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<EmployeeDetailDto>>> GetEmployeeDetailById(int id, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetEmployeeDetailByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<EmployeeDetailDto>.Success("Lấy thông tin chi tiết nhân viên thành công", employee));
    }

    [HttpGet("{id:int}/orders")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<PageResponse<EmployeeOrderDto>>>> GetEmployeeOrders(
        int id,
        [FromQuery] int pageNo = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] DateOnly? dateFrom = null,
        [FromQuery] DateOnly? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        DateTime? from = dateFrom?.ToDateTime(TimeOnly.MinValue);
        DateTime? to = dateTo?.ToDateTime(TimeOnly.MaxValue);
        var orders = await _employeeService.GetOrdersByEmployeeIdAsync(id, status, from, to, Math.Max(0, pageNo), Math.Max(1, Math.Min(100, pageSize)), cancellationToken);
        return Ok(ApiResponse<PageResponse<EmployeeOrderDto>>.Success("Lấy danh sách đơn hàng của nhân viên thành công", orders));
    }

    [HttpGet("user/{userId:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetEmployeeByUserId(int userId, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.GetEmployeeByUserIdAsync(userId, cancellationToken);
        return Ok(ApiResponse<EmployeeDto>.Success("Lấy thông tin nhân viên thành công", employee));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> UpdateEmployee(int id, [FromBody] EmployeeDto request, CancellationToken cancellationToken)
    {
        var employee = await _employeeService.UpdateEmployeeByAdminAsync(id, request, cancellationToken);
        return Ok(ApiResponse<EmployeeDto>.Success("Cập nhật nhân viên thành công", employee));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEmployee(int id, CancellationToken cancellationToken)
    {
        await _employeeService.DeleteEmployeeAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Success("Xóa nhân viên thành công", null));
    }

    [HttpGet("me")]
    [Authorize(Roles = "EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetMyProfile(CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("Yêu cầu đăng nhập");
        var employee = await _employeeService.GetMyProfileAsync(username, cancellationToken);
        return Ok(ApiResponse<EmployeeDto>.Success("Lấy thông tin cá nhân thành công", employee));
    }

    [HttpPut("me")]
    [Authorize(Roles = "EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> UpdateMyProfile([FromBody] EmployeeDto request, CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("Yêu cầu đăng nhập");
        var employee = await _employeeService.UpdateMyProfileAsync(username, request, cancellationToken);
        return Ok(ApiResponse<EmployeeDto>.Success("Cập nhật thông tin cá nhân thành công", employee));
    }
}
