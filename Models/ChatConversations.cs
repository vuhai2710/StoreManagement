using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class ChatConversations
{
    public int IdConversation { get; set; }

    public int IdCustomer { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Thời gian admin/employee xem conversation lần cuối
    /// </summary>
    public DateTime? LastViewedByAdminAt { get; set; }

    /// <summary>
    /// Thời gian customer xem conversation lần cuối
    /// </summary>
    public DateTime? LastViewedByCustomerAt { get; set; }

    public virtual ICollection<ChatMessages> ChatMessages { get; set; } = new List<ChatMessages>();

    public virtual Customers IdCustomerNavigation { get; set; } = null!;
}
