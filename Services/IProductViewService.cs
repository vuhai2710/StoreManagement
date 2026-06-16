namespace StoreManagement.Services;

public interface IProductViewService
{
    Task LogViewAsync(int? userId, string? sessionId, int productId, CancellationToken cancellationToken = default);
}
