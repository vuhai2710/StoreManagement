using Microsoft.EntityFrameworkCore;
using StoreManagement.Common;
using StoreManagement.Data;
using StoreManagement.Extensions;
using StoreManagement.Models;

namespace StoreManagement.Repositories;

public interface IOrderDetailRepository
{
    Task<OrderDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

public class OrderDetailRepository : IOrderDetailRepository
{
    private readonly AppDbContext _dbContext;
    public OrderDetailRepository(AppDbContext dbContext) => _dbContext = dbContext;
    public Task<OrderDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.OrderDetails
            .Include(x => x.IdOrderNavigation)
            .Include(x => x.IdProductNavigation)
            .FirstOrDefaultAsync(x => x.IdOrderDetail == id, cancellationToken);
}

public interface IShipmentRepository
{
    Task<Shipments?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Shipments?> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<Shipments?> GetByGhnOrderCodeAsync(string ghnOrderCode, CancellationToken cancellationToken = default);
    Task AddAsync(Shipments shipment, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class ShipmentRepository : IShipmentRepository
{
    private readonly AppDbContext _dbContext;
    public ShipmentRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<Shipments?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Shipments.Include(x => x.IdOrderNavigation).FirstOrDefaultAsync(x => x.IdShipment == id, cancellationToken);

    public Task<Shipments?> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default) =>
        _dbContext.Shipments.Include(x => x.IdOrderNavigation).FirstOrDefaultAsync(x => x.IdOrder == orderId, cancellationToken);

    public Task<Shipments?> GetByGhnOrderCodeAsync(string ghnOrderCode, CancellationToken cancellationToken = default) =>
        _dbContext.Shipments.Include(x => x.IdOrderNavigation).FirstOrDefaultAsync(x => x.GhnOrderCode == ghnOrderCode, cancellationToken);

    public Task AddAsync(Shipments shipment, CancellationToken cancellationToken = default) =>
        _dbContext.Shipments.AddAsync(shipment, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}

public interface IChatConversationRepository
{
    Task<ChatConversations?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ChatConversations?> GetOpenByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<PagedResult<ChatConversations>> GetPagedAsync(int pageNo, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(ChatConversations conversation, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class ChatConversationRepository : IChatConversationRepository
{
    private readonly AppDbContext _dbContext;
    public ChatConversationRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<ChatConversations?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.ChatConversations
            .Include(x => x.IdCustomerNavigation)
            .Include(x => x.ChatMessages.OrderByDescending(m => m.CreatedAt))
            .FirstOrDefaultAsync(x => x.IdConversation == id, cancellationToken);

    public Task<ChatConversations?> GetOpenByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default) =>
        _dbContext.ChatConversations
            .Include(x => x.IdCustomerNavigation)
            .Include(x => x.ChatMessages.OrderByDescending(m => m.CreatedAt))
            .FirstOrDefaultAsync(x => x.IdCustomer == customerId && x.Status == "OPEN", cancellationToken);

    public Task<PagedResult<ChatConversations>> GetPagedAsync(int pageNo, int pageSize, CancellationToken cancellationToken = default) =>
        _dbContext.ChatConversations
            .AsNoTracking()
            .Include(x => x.IdCustomerNavigation)
            .Include(x => x.ChatMessages.OrderByDescending(m => m.CreatedAt))
            .OrderByDescending(x => x.UpdatedAt)
            .ToPagedResultAsync(pageNo, pageSize, cancellationToken);

    public Task AddAsync(ChatConversations conversation, CancellationToken cancellationToken = default) =>
        _dbContext.ChatConversations.AddAsync(conversation, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}

public interface IChatMessageRepository
{
    Task<PagedResult<ChatMessages>> GetByConversationAsync(int conversationId, int pageNo, int pageSize, CancellationToken cancellationToken = default);
    Task<ChatMessages?> GetLatestByConversationIdAsync(int conversationId, CancellationToken cancellationToken = default);
    Task<long> CountUnreadCustomerMessagesAsync(int conversationId, DateTime from, CancellationToken cancellationToken = default);
    Task<long> CountUnreadStaffMessagesAsync(int conversationId, DateTime from, CancellationToken cancellationToken = default);
    Task AddAsync(ChatMessages message, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly AppDbContext _dbContext;
    public ChatMessageRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<PagedResult<ChatMessages>> GetByConversationAsync(int conversationId, int pageNo, int pageSize, CancellationToken cancellationToken = default) =>
        _dbContext.ChatMessages
            .AsNoTracking()
            .Where(x => x.IdConversation == conversationId)
            .OrderByDescending(x => x.CreatedAt)
            .ToPagedResultAsync(pageNo, pageSize, cancellationToken);

    public Task<ChatMessages?> GetLatestByConversationIdAsync(int conversationId, CancellationToken cancellationToken = default) =>
        _dbContext.ChatMessages.OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(x => x.IdConversation == conversationId, cancellationToken);

    public Task<long> CountUnreadCustomerMessagesAsync(int conversationId, DateTime from, CancellationToken cancellationToken = default) =>
        _dbContext.ChatMessages.LongCountAsync(x => x.IdConversation == conversationId && x.CreatedAt >= from && x.SenderType == "CUSTOMER", cancellationToken);

    public Task<long> CountUnreadStaffMessagesAsync(int conversationId, DateTime from, CancellationToken cancellationToken = default) =>
        _dbContext.ChatMessages.LongCountAsync(x => x.IdConversation == conversationId && x.CreatedAt >= from && (x.SenderType == "ADMIN" || x.SenderType == "EMPLOYEE"), cancellationToken);

    public Task AddAsync(ChatMessages message, CancellationToken cancellationToken = default) =>
        _dbContext.ChatMessages.AddAsync(message, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrders?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<PurchaseOrders>> SearchAsync(string? keyword, int? supplierId, DateTime? startDate, DateTime? endDate, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task AddAsync(PurchaseOrders order, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly AppDbContext _dbContext;
    public PurchaseOrderRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<PurchaseOrders?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.PurchaseOrders
            .Include(x => x.IdSupplierNavigation)
            .Include(x => x.IdEmployeeNavigation)
            .Include(x => x.PurchaseOrderDetails)
                .ThenInclude(x => x.IdProductNavigation)
            .FirstOrDefaultAsync(x => x.IdPurchaseOrder == id, cancellationToken);

    public Task<PagedResult<PurchaseOrders>> SearchAsync(string? keyword, int? supplierId, DateTime? startDate, DateTime? endDate, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PurchaseOrders
            .AsNoTracking()
            .Include(x => x.IdSupplierNavigation)
            .Include(x => x.IdEmployeeNavigation)
            .Include(x => x.PurchaseOrderDetails)
                .ThenInclude(x => x.IdProductNavigation)
            .AsQueryable();

        if (supplierId.HasValue)
        {
            query = query.Where(x => x.IdSupplier == supplierId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(x => x.OrderDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.OrderDate <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var term = keyword.Trim();
            query = query.Where(x =>
                (x.IdSupplierNavigation != null && x.IdSupplierNavigation.SupplierName != null && EF.Functions.Like(x.IdSupplierNavigation.SupplierName, $"%{term}%")) ||
                x.IdPurchaseOrder.ToString().Contains(term));
        }

        query = query.ApplySorting(sortBy, sortDirection);
        return query.ToPagedResultAsync(pageNo, pageSize, cancellationToken);
    }

    public Task AddAsync(PurchaseOrders order, CancellationToken cancellationToken = default) =>
        _dbContext.PurchaseOrders.AddAsync(order, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}

public interface IProductViewRepository
{
    Task<List<int>> GetTopViewedProductIdsAsync(int? userId, int limit, CancellationToken cancellationToken = default);
}

public class ProductViewRepository : IProductViewRepository
{
    private readonly AppDbContext _dbContext;
    public ProductViewRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<List<int>> GetTopViewedProductIdsAsync(int? userId, int limit, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ProductView.AsQueryable();
        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        return await query
            .GroupBy(x => x.ProductId)
            .OrderByDescending(x => x.Count())
            .Take(limit)
            .Select(x => x.Key)
            .ToListAsync(cancellationToken);
    }
}