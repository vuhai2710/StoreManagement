using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Chat;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ICustomerService _customerService;
    private readonly ICurrentUserContext _currentUserContext;

    public ChatController(IChatService chatService, ICustomerService customerService, ICurrentUserContext currentUserContext)
    {
        _chatService = chatService;
        _customerService = customerService;
        _currentUserContext = currentUserContext;
    }

    [HttpPost("conversations")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<ChatConversationDto>>> CreateConversation(CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetCustomerByUsernameAsync(_currentUserContext.GetRequiredUsername(), cancellationToken);
        var result = await _chatService.CreateConversationAsync(customer.IdCustomer!.Value, cancellationToken);
        return Ok(ApiResponse<ChatConversationDto>.Success("Tạo cuộc hội thoại thành công", result));
    }

    [HttpGet("conversations/my")]
    [Authorize(Roles = "CUSTOMER")]
    public async Task<ActionResult<ApiResponse<ChatConversationDto>>> GetMyConversation(CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetCustomerByUsernameAsync(_currentUserContext.GetRequiredUsername(), cancellationToken);
        var result = await _chatService.GetOrCreateCustomerConversationAsync(customer.IdCustomer!.Value, cancellationToken);
        return Ok(ApiResponse<ChatConversationDto>.Success("Lấy cuộc hội thoại thành công", result));
    }

    [HttpGet("conversations")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<ChatConversationDto>>>> GetAllConversations([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _chatService.GetAllConversationsAsync(pageNo, pageSize, cancellationToken);
        return Ok(ApiResponse<PageResponse<ChatConversationDto>>.Success("Lấy danh sách cuộc hội thoại thành công", result));
    }

    [HttpGet("conversations/{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<ChatConversationDto>>> GetConversationById(int id, CancellationToken cancellationToken)
    {
        var result = await _chatService.GetConversationByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ChatConversationDto>.Success("Lấy thông tin cuộc hội thoại thành công", result));
    }

    [HttpGet("conversations/{id:int}/messages")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<PageResponse<ChatMessageDto>>>> GetConversationMessages(int id, [FromQuery] int pageNo = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var result = await _chatService.GetConversationMessagesAsync(id, pageNo, pageSize, cancellationToken);
        return Ok(ApiResponse<PageResponse<ChatMessageDto>>.Success("Lấy lịch sử tin nhắn thành công", result));
    }

    [HttpPut("conversations/{id:int}/close")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<object>>> CloseConversation(int id, CancellationToken cancellationToken)
    {
        await _chatService.CloseConversationAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Success("Đóng cuộc hội thoại thành công", null));
    }

    [HttpPut("conversations/{id:int}/mark-viewed")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<object>>> MarkConversationAsViewed(int id, CancellationToken cancellationToken)
    {
        await _chatService.MarkConversationAsViewedAsync(id, _currentUserContext.IsInRole("CUSTOMER"), cancellationToken);
        return Ok(ApiResponse<object>.Success("Đã đánh dấu cuộc hội thoại đã xem", null));
    }

    [HttpPost("conversations/customer/{customerId:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<ChatConversationDto>>> GetOrCreateConversationForCustomer(int customerId, CancellationToken cancellationToken)
    {
        var result = await _chatService.GetOrCreateCustomerConversationAsync(customerId, cancellationToken);
        return Ok(ApiResponse<ChatConversationDto>.Success("Lấy hoặc tạo cuộc hội thoại thành công", result));
    }
}