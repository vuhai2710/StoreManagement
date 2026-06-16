using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.User;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<PageResponse<UserDto>>>> GetAllUsers(
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 5,
        [FromQuery] string sortBy = "IdUser",
        [FromQuery] string sortDirection = "ASC",
        CancellationToken cancellationToken = default)
    {
        var userPage = await _userService.GetAllUsersPaginatedAsync(pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<UserDto>>.Success("Lấy danh sách user thành công", userPage));
    }

    [HttpGet("status")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<PageResponse<UserDto>>>> GetUsersByStatus(
        [FromQuery] bool isActive,
        [FromQuery] int pageNo = 1,
        [FromQuery] int pageSize = 5,
        [FromQuery] string sortBy = "IdUser",
        [FromQuery] string sortDirection = "ASC",
        CancellationToken cancellationToken = default)
    {
        var userPage = await _userService.GetUsersByStatusAsync(isActive, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<UserDto>>.Success("Lấy danh sách user theo trạng thái thành công", userPage));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(int id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<UserDto>.Success("Lấy thông tin user thành công", user));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(int id, [FromBody] UserDto userDto, CancellationToken cancellationToken)
    {
        var updatedUser = await _userService.UpdateUserAsync(id, userDto, cancellationToken);
        return Ok(ApiResponse<UserDto>.Success("Cập nhật user thành công", updatedUser));
    }

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<object>>> DeactivateUser(int id, CancellationToken cancellationToken)
    {
        await _userService.DeactivateUserAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Success("Vô hiệu hóa user thành công", null));
    }

    [HttpPatch("{id:int}/activate")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<object>>> ActivateUser(int id, CancellationToken cancellationToken)
    {
        await _userService.ActivateUserAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Success("Kích hoạt user thành công", null));
    }

    [HttpPatch("{id:int}/role")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<UserDto>>> ChangeUserRole(int id, [FromQuery] string role, CancellationToken cancellationToken)
    {
        var updatedUser = await _userService.ChangeUserRoleAsync(id, role, cancellationToken);
        return Ok(ApiResponse<UserDto>.Success("Thay đổi role thành công", updatedUser));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id, CancellationToken cancellationToken)
    {
        await _userService.DeleteUserAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Success("Xóa user thành công", null));
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetMyProfile(CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("Yêu cầu đăng nhập");
        var user = await _userService.GetUserByUsernameAsync(username, cancellationToken);
        return Ok(ApiResponse<UserDto>.Success("Lấy thông tin profile thành công", user));
    }

    [HttpPost("avatar")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UploadAvatar([FromForm] IFormFile avatar, CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("Yêu cầu đăng nhập");
        var updatedUser = await _userService.UploadAvatarAsync(username, avatar, cancellationToken);
        return Ok(ApiResponse<UserDto>.Success("Upload ảnh đại diện thành công", updatedUser));
    }

    [HttpPut("avatar")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateAvatar([FromForm] IFormFile avatar, CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("Yêu cầu đăng nhập");
        var updatedUser = await _userService.UpdateAvatarAsync(username, avatar, cancellationToken);
        return Ok(ApiResponse<UserDto>.Success("Cập nhật ảnh đại diện thành công", updatedUser));
    }

    [HttpDelete("avatar")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAvatar(CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? throw new UnauthorizedAccessException("Yêu cầu đăng nhập");
        await _userService.DeleteAvatarAsync(username, cancellationToken);
        return Ok(ApiResponse<object>.Success("Xóa ảnh đại diện thành công", null));
    }
}
