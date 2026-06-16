namespace StoreManagement.Dtos.Chat;

public class ChatConversationDto
{
    public int? IdConversation { get; set; }
    public int? IdCustomer { get; set; }
    public string? CustomerName { get; set; }
    public string? Status { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageTime { get; set; }
    public long? UnreadCount { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ChatMessageDto
{
    public int? IdMessage { get; set; }
    public int? ConversationId { get; set; }
    public int? SenderId { get; set; }
    public string? SenderType { get; set; }
    public string? SenderName { get; set; }
    public string? SenderRole { get; set; }
    public string? Message { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class ChatMessageRequestDto
{
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string SenderType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}