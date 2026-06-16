using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class Users
{
    public int IdUser { get; set; }

    public string Username { get; set; } = null!;

    /// <summary>
    /// Lưu mật khẩu đã mã hoá (BCrypt/Argon2)
    /// </summary>
    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// URL ảnh đại diện của user
    /// </summary>
    public string? AvatarUrl { get; set; }

    public virtual Customers? Customers { get; set; }

    public virtual Employees? Employees { get; set; }

    public virtual ICollection<Notifications> Notifications { get; set; } = new List<Notifications>();

    public virtual ICollection<PasswordResetTokens> PasswordResetTokens { get; set; } = new List<PasswordResetTokens>();
}
