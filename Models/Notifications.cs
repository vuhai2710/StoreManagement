using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

/// <summary>
/// [DEPRECATED] Notification module removed. Table preserved for historical data.
/// </summary>
public partial class Notifications
{
    public int IdNotification { get; set; }

    /// <summary>
    /// User nhận thông báo
    /// </summary>
    public int IdUser { get; set; }

    /// <summary>
    /// Loại thông báo
    /// </summary>
    public string NotificationType { get; set; } = null!;

    /// <summary>
    /// Tiêu đề thông báo
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Nội dung thông báo
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Loại đối tượng liên quan
    /// </summary>
    public string? ReferenceType { get; set; }

    /// <summary>
    /// ID đối tượng liên quan
    /// </summary>
    public int? ReferenceId { get; set; }

    /// <summary>
    /// Đã đọc chưa
    /// </summary>
    public bool? IsRead { get; set; }

    /// <summary>
    /// Đã gửi email chưa
    /// </summary>
    public bool? SentEmail { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Users IdUserNavigation { get; set; } = null!;
}
