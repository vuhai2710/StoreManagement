using StoreManagement.Common;
using StoreManagement.Dtos.Chat;
using StoreManagement.Exceptions;
using StoreManagement.Extensions;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public interface IChatService
{
    Task<ChatConversationDto> CreateConversationAsync(int customerId, CancellationToken cancellationToken = default);
    Task<ChatConversationDto> GetOrCreateCustomerConversationAsync(int customerId, CancellationToken cancellationToken = default);
    Task<PageResponse<ChatConversationDto>> GetAllConversationsAsync(int pageNo, int pageSize, CancellationToken cancellationToken = default);
    Task<ChatConversationDto> GetConversationByIdAsync(int conversationId, CancellationToken cancellationToken = default);
    Task<PageResponse<ChatMessageDto>> GetConversationMessagesAsync(int conversationId, int pageNo, int pageSize, CancellationToken cancellationToken = default);
    Task CloseConversationAsync(int conversationId, CancellationToken cancellationToken = default);
    Task MarkConversationAsViewedAsync(int conversationId, bool viewedByCustomer, CancellationToken cancellationToken = default);
}

public class ChatService : IChatService
{
    private readonly IChatConversationRepository _conversationRepository;
    private readonly IChatMessageRepository _messageRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public ChatService(
        IChatConversationRepository conversationRepository,
        IChatMessageRepository messageRepository,
        ICustomerRepository customerRepository,
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _customerRepository = customerRepository;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<ChatConversationDto> CreateConversationAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Khách hàng không tồn tại với ID: {customerId}");

        var existing = await _conversationRepository.GetOpenByCustomerIdAsync(customerId, cancellationToken);
        if (existing is not null)
        {
            return await MapConversationAsync(existing, cancellationToken);
        }

        var conversation = new Models.ChatConversations
        {
            IdCustomer = customerId,
            Status = "OPEN",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _conversationRepository.AddAsync(conversation, cancellationToken);
        await _conversationRepository.SaveChangesAsync(cancellationToken);
        var saved = await _conversationRepository.GetByIdAsync(conversation.IdConversation, cancellationToken)
            ?? throw new ResourceNotFoundException("Không thể tải cuộc hội thoại vừa tạo");
        return await MapConversationAsync(saved, cancellationToken);
    }

    public async Task<ChatConversationDto> GetOrCreateCustomerConversationAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var existing = await _conversationRepository.GetOpenByCustomerIdAsync(customerId, cancellationToken);
        return existing is not null
            ? await MapConversationAsync(existing, cancellationToken)
            : await CreateConversationAsync(customerId, cancellationToken);
    }

    public async Task<PageResponse<ChatConversationDto>> GetAllConversationsAsync(int pageNo, int pageSize, CancellationToken cancellationToken = default)
    {
        var page = await _conversationRepository.GetPagedAsync(pageNo, pageSize, cancellationToken);
        var items = new List<ChatConversationDto>();
        foreach (var conversation in page.Items)
        {
            items.Add(await MapConversationAsync(conversation, cancellationToken));
        }

        return new PagedResult<ChatConversationDto>
        {
            Items = items,
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<ChatConversationDto> GetConversationByIdAsync(int conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Cuộc hội thoại không tồn tại với ID: {conversationId}");
        return await MapConversationAsync(conversation, cancellationToken);
    }

    public async Task<PageResponse<ChatMessageDto>> GetConversationMessagesAsync(int conversationId, int pageNo, int pageSize, CancellationToken cancellationToken = default)
    {
        _ = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Cuộc hội thoại không tồn tại với ID: {conversationId}");

        var page = await _messageRepository.GetByConversationAsync(conversationId, pageNo, pageSize, cancellationToken);
        var items = new List<ChatMessageDto>();
        foreach (var message in page.Items.OrderBy(x => x.CreatedAt))
        {
            items.Add(await MapMessageAsync(message, cancellationToken));
        }

        return new PagedResult<ChatMessageDto>
        {
            Items = items,
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task CloseConversationAsync(int conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Cuộc hội thoại không tồn tại với ID: {conversationId}");
        conversation.Status = "CLOSED";
        conversation.UpdatedAt = DateTime.UtcNow;
        await _conversationRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkConversationAsViewedAsync(int conversationId, bool viewedByCustomer, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Cuộc hội thoại không tồn tại với ID: {conversationId}");

        if (viewedByCustomer)
        {
            conversation.LastViewedByCustomerAt = DateTime.UtcNow;
        }
        else
        {
            conversation.LastViewedByAdminAt = DateTime.UtcNow;
        }

        conversation.UpdatedAt = DateTime.UtcNow;
        await _conversationRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<ChatConversationDto> MapConversationAsync(Models.ChatConversations conversation, CancellationToken cancellationToken)
    {
        var lastMessage = conversation.ChatMessages.OrderByDescending(x => x.CreatedAt).FirstOrDefault()
            ?? await _messageRepository.GetLatestByConversationIdAsync(conversation.IdConversation, cancellationToken);

        var unreadCount = 0L;
        if (conversation.LastViewedByAdminAt.HasValue)
        {
            unreadCount = await _messageRepository.CountUnreadCustomerMessagesAsync(conversation.IdConversation, conversation.LastViewedByAdminAt.Value, cancellationToken);
        }
        else if (conversation.LastViewedByCustomerAt.HasValue)
        {
            unreadCount = await _messageRepository.CountUnreadStaffMessagesAsync(conversation.IdConversation, conversation.LastViewedByCustomerAt.Value, cancellationToken);
        }

        return new ChatConversationDto
        {
            IdConversation = conversation.IdConversation,
            IdCustomer = conversation.IdCustomer,
            CustomerName = conversation.IdCustomerNavigation?.CustomerName,
            Status = conversation.Status,
            LastMessage = lastMessage?.Message,
            LastMessageTime = lastMessage?.CreatedAt,
            UnreadCount = unreadCount,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt
        };
    }

    private async Task<ChatMessageDto> MapMessageAsync(Models.ChatMessages message, CancellationToken cancellationToken)
    {
        string? senderName = null;
        string? senderRole = message.SenderType;
        var user = await _userRepository.GetByIdAsync(message.SenderId, cancellationToken);
        if (user is not null)
        {
            senderRole = user.Role ?? message.SenderType;
        }

        if (string.Equals(message.SenderType, "CUSTOMER", StringComparison.OrdinalIgnoreCase))
        {
            var customer = await _customerRepository.GetByUserIdAsync(message.SenderId, cancellationToken);
            senderName = customer?.CustomerName ?? user?.Username;
        }
        else
        {
            var employee = await _employeeRepository.GetByUserIdAsync(message.SenderId, cancellationToken);
            senderName = employee?.EmployeeName ?? user?.Username;
        }

        return new ChatMessageDto
        {
            IdMessage = message.IdMessage,
            ConversationId = message.IdConversation,
            SenderId = message.SenderId,
            SenderType = message.SenderType,
            SenderName = senderName,
            SenderRole = senderRole,
            Message = message.Message,
            CreatedAt = message.CreatedAt
        };
    }
}