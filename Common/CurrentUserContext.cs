using System.Security.Claims;

namespace StoreManagement.Common;

public interface ICurrentUserContext
{
    string GetRequiredUsername();
    int GetRequiredUserId();
    int? GetEmployeeId();
    string? GetRole();
    bool IsInRole(string role);
}

public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetRequiredUsername()
    {
        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        return string.IsNullOrWhiteSpace(username)
            ? throw new InvalidOperationException("Không thể xác định người dùng hiện tại")
            : username;
    }

    public int GetRequiredUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("idUser");
        if (!int.TryParse(claim, out var userId))
        {
            throw new InvalidOperationException("Không thể xác định user hiện tại");
        }

        return userId;
    }

    public int? GetEmployeeId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("employeeId");
        return int.TryParse(claim, out var employeeId) ? employeeId : null;
    }

    public string? GetRole()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirstValue("role");
    }

    public bool IsInRole(string role)
    {
        return string.Equals(GetRole(), role, StringComparison.OrdinalIgnoreCase);
    }
}