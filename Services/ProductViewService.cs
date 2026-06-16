namespace StoreManagement.Services;

public class ProductViewService : IProductViewService
{
    private readonly ILogger<ProductViewService> _logger;

    public ProductViewService(ILogger<ProductViewService> logger)
    {
        _logger = logger;
    }

    public Task LogViewAsync(int? userId, string? sessionId, int productId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Product view logged: userId={UserId}, sessionId={SessionId}, productId={ProductId}", userId, sessionId, productId);
        return Task.CompletedTask;
    }
}
