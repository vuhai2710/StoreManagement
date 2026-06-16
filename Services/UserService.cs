using BCrypt.Net;
using StoreManagement.Common;
using StoreManagement.Dtos.User;
using StoreManagement.Exceptions;
using StoreManagement.Extensions;
using StoreManagement.Models;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IFileStorageService _fileStorageService;

    public UserService(IUserRepository userRepository, ICustomerRepository customerRepository, IFileStorageService fileStorageService)
    {
        _userRepository = userRepository;
        _customerRepository = customerRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<PageResponse<UserDto>> GetAllUsersPaginatedAsync(int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var page = await _userRepository.GetPagedAsync(pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<UserDto>
        {
            Items = page.Items.Select(MapToDto).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<PageResponse<UserDto>> GetUsersByStatusAsync(bool isActive, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var page = await _userRepository.GetByStatusPagedAsync(isActive, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<UserDto>
        {
            Items = page.Items.Select(MapToDto).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<UserDto> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"User không tồn tại với ID: {id}");
        return MapToDto(user);
    }

    public async Task<UserDto> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken)
            ?? throw new ResourceNotFoundException($"User không tồn tại với username: {username}");
        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(int id, UserDto userDto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"User không tồn tại với ID: {id}");

        if (!string.IsNullOrWhiteSpace(userDto.Email) && !string.Equals(userDto.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _userRepository.GetByEmailAsync(userDto.Email, cancellationToken);
            if (existing is not null && existing.IdUser != id)
            {
                throw new ConflictException($"Email đã được sử dụng: {userDto.Email}");
            }

            user.Email = userDto.Email;
        }

        if (!string.IsNullOrWhiteSpace(userDto.Role))
        {
            user.Role = userDto.Role.ToUpperInvariant();
        }

        if (userDto.IsActive.HasValue)
        {
            user.IsActive = userDto.IsActive.Value;
        }

        await _userRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(user);
    }

    public async Task DeactivateUserAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"User không tồn tại với ID: {id}");
        user.IsActive = false;
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ActivateUserAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"User không tồn tại với ID: {id}");
        user.IsActive = true;
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto> ChangeUserRoleAsync(int id, string role, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"User không tồn tại với ID: {id}");

        var normalized = role.ToUpperInvariant();
        if (normalized is not ("ADMIN" or "EMPLOYEE" or "CUSTOMER"))
        {
            throw new InvalidOperationException($"Role không hợp lệ: {role}");
        }

        user.Role = normalized;
        await _userRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(user);
    }

    public async Task DeleteUserAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"User không tồn tại với ID: {id}");

        var customer = await _customerRepository.GetByUserIdAsync(id, cancellationToken);
        if (customer is not null)
        {
            await _customerRepository.DeleteAsync(customer, cancellationToken);
            await _customerRepository.SaveChangesAsync(cancellationToken);
        }

        await _userRepository.DeleteAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangePasswordAsync(string username, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken)
            ?? throw new ResourceNotFoundException("User không tồn tại");

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Password))
        {
            throw new InvalidOperationException("Mật khẩu hiện tại không đúng");
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto> UploadAvatarAsync(string username, IFormFile avatar, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken)
            ?? throw new ResourceNotFoundException("User không tồn tại");

        user.AvatarUrl = await _fileStorageService.SaveImageAsync(avatar, "users", cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(user);
    }

    public async Task<UserDto> UpdateAvatarAsync(string username, IFormFile avatar, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken)
            ?? throw new ResourceNotFoundException("User không tồn tại");

        if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
        {
            await _fileStorageService.DeleteImageAsync(user.AvatarUrl, cancellationToken);
        }

        user.AvatarUrl = await _fileStorageService.SaveImageAsync(avatar, "users", cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(user);
    }

    public async Task DeleteAvatarAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken)
            ?? throw new ResourceNotFoundException("User không tồn tại");

        if (string.IsNullOrWhiteSpace(user.AvatarUrl))
        {
            throw new InvalidOperationException("User không có ảnh đại diện");
        }

        await _fileStorageService.DeleteImageAsync(user.AvatarUrl, cancellationToken);
        user.AvatarUrl = null;
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    private static UserDto MapToDto(Users user)
    {
        return new UserDto
        {
            IdUser = user.IdUser,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
