using StoreManagement.Common;
using StoreManagement.Dtos.User;

namespace StoreManagement.Services;

public interface IUserService
{
    Task<PageResponse<UserDto>> GetAllUsersPaginatedAsync(int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<PageResponse<UserDto>> GetUsersByStatusAsync(bool isActive, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<UserDto> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserDto> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateUserAsync(int id, UserDto userDto, CancellationToken cancellationToken = default);
    Task DeactivateUserAsync(int id, CancellationToken cancellationToken = default);
    Task ActivateUserAsync(int id, CancellationToken cancellationToken = default);
    Task<UserDto> ChangeUserRoleAsync(int id, string role, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(int id, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(string username, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    Task<UserDto> UploadAvatarAsync(string username, IFormFile avatar, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateAvatarAsync(string username, IFormFile avatar, CancellationToken cancellationToken = default);
    Task DeleteAvatarAsync(string username, CancellationToken cancellationToken = default);
}
