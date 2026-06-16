using System;
using System.Collections.Generic;

namespace StoreManagement.Models;

public partial class ChatMessages
{
    public int IdMessage { get; set; }

    public int IdConversation { get; set; }

    /// <summary>
    /// ID of user (customer/employee/admin)
    /// </summary>
    public int SenderId { get; set; }

    public string SenderType { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ChatConversations IdConversationNavigation { get; set; } = null!;
}
