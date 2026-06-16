using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace StoreManagement.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public Task JoinConversation(string conversationId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"conversation:{conversationId}");
    }

    public Task LeaveConversation(string conversationId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation:{conversationId}");
    }

    public Task SendMessage(string conversationId, object payload)
    {
        return Clients.Group($"conversation:{conversationId}").SendAsync("messageReceived", payload);
    }

    public Task MarkViewed(string conversationId, object payload)
    {
        return Clients.Group($"conversation:{conversationId}").SendAsync("conversationViewed", payload);
    }

    public Task CloseConversation(string conversationId, object payload)
    {
        return Clients.Group($"conversation:{conversationId}").SendAsync("conversationClosed", payload);
    }
}
