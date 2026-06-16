using StoreManagement.Common;
using StoreManagement.Dtos.Order;
using StoreManagement.Exceptions;
using StoreManagement.Extensions;
using StoreManagement.Models;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public interface IOrderReturnService
{
    Task<OrderReturnDto> RequestReturnAsync(int customerId, int orderId, OrderReturnDto request, CancellationToken cancellationToken = default);
    Task<OrderReturnDto> RequestExchangeAsync(int customerId, int orderId, OrderReturnDto request, CancellationToken cancellationToken = default);
    Task<OrderReturnDto> GetOrderReturnByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PageResponse<OrderReturnDto>> GetMyReturnsAsync(int customerId, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<PageResponse<OrderReturnDto>> GetAllReturnsAsync(int pageNo, int pageSize, string sortBy, string sortDirection, string? status, string? returnType, string? keyword, string? customerKeyword, CancellationToken cancellationToken = default);
    Task<bool> HasActiveReturnRequestAsync(int orderId, CancellationToken cancellationToken = default);
    Task<OrderReturnDto> ApproveAsync(int idReturn, int? employeeId, string? noteAdmin, decimal? refundAmount, CancellationToken cancellationToken = default);
    Task<OrderReturnDto> RejectAsync(int idReturn, int? employeeId, string? noteAdmin, CancellationToken cancellationToken = default);
    Task<OrderReturnDto> CompleteAsync(int idReturn, int? employeeId, CancellationToken cancellationToken = default);
}

public class OrderReturnService : IOrderReturnService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly IOrderReturnRepository _orderReturnRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public OrderReturnService(
        IOrderRepository orderRepository,
        IOrderDetailRepository orderDetailRepository,
        IOrderReturnRepository orderReturnRepository,
        IProductRepository productRepository,
        IInventoryTransactionRepository inventoryTransactionRepository,
        ICustomerRepository customerRepository,
        IEmployeeRepository employeeRepository)
    {
        _orderRepository = orderRepository;
        _orderDetailRepository = orderDetailRepository;
        _orderReturnRepository = orderReturnRepository;
        _productRepository = productRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _customerRepository = customerRepository;
        _employeeRepository = employeeRepository;
    }

    public Task<OrderReturnDto> RequestReturnAsync(int customerId, int orderId, OrderReturnDto request, CancellationToken cancellationToken = default) =>
        CreateReturnRequestAsync(customerId, orderId, request, "RETURN", cancellationToken);

    public Task<OrderReturnDto> RequestExchangeAsync(int customerId, int orderId, OrderReturnDto request, CancellationToken cancellationToken = default) =>
        CreateReturnRequestAsync(customerId, orderId, request, "EXCHANGE", cancellationToken);

    public async Task<OrderReturnDto> GetOrderReturnByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await LoadReturnAsync(id, cancellationToken);
        return Map(entity);
    }

    public async Task<PageResponse<OrderReturnDto>> GetMyReturnsAsync(int customerId, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var page = await _orderReturnRepository.GetMyReturnsAsync(customerId, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<OrderReturnDto>
        {
            Items = page.Items.Select(Map).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<PageResponse<OrderReturnDto>> GetAllReturnsAsync(int pageNo, int pageSize, string sortBy, string sortDirection, string? status, string? returnType, string? keyword, string? customerKeyword, CancellationToken cancellationToken = default)
    {
        var page = await _orderReturnRepository.SearchAsync(status, returnType, keyword, customerKeyword, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<OrderReturnDto>
        {
            Items = page.Items.Select(Map).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public Task<bool> HasActiveReturnRequestAsync(int orderId, CancellationToken cancellationToken = default) =>
        _orderReturnRepository.ExistsActiveReturnByOrderIdAsync(orderId, cancellationToken);

    public async Task<OrderReturnDto> ApproveAsync(int idReturn, int? employeeId, string? noteAdmin, decimal? refundAmount, CancellationToken cancellationToken = default)
    {
        var entity = await LoadReturnAsync(idReturn, cancellationToken);
        if (!string.Equals(entity.Status, "REQUESTED", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Chỉ có thể duyệt yêu cầu ở trạng thái REQUESTED");
        }

        if (employeeId.HasValue)
        {
            _ = await _employeeRepository.GetByIdAsync(employeeId.Value, cancellationToken)
                ?? throw new ResourceNotFoundException("Không tìm thấy nhân viên");
        }

        entity.ProcessedByEmployeeId = employeeId;
        entity.NoteAdmin = noteAdmin;
        if (refundAmount.HasValue)
        {
            entity.RefundAmount = refundAmount.Value;
        }
        entity.Status = "APPROVED";
        entity.UpdatedAt = DateTime.UtcNow;
        await _orderReturnRepository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<OrderReturnDto> RejectAsync(int idReturn, int? employeeId, string? noteAdmin, CancellationToken cancellationToken = default)
    {
        var entity = await LoadReturnAsync(idReturn, cancellationToken);
        if (!string.Equals(entity.Status, "REQUESTED", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Chỉ có thể từ chối yêu cầu ở trạng thái REQUESTED");
        }

        entity.ProcessedByEmployeeId = employeeId;
        entity.NoteAdmin = noteAdmin;
        entity.Status = "REJECTED";
        entity.UpdatedAt = DateTime.UtcNow;
        await _orderReturnRepository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<OrderReturnDto> CompleteAsync(int idReturn, int? employeeId, CancellationToken cancellationToken = default)
    {
        var entity = await LoadReturnAsync(idReturn, cancellationToken);
        if (!string.Equals(entity.Status, "APPROVED", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Chỉ có thể hoàn tất yêu cầu ở trạng thái APPROVED");
        }

        entity.ProcessedByEmployeeId = employeeId;
        if (string.Equals(entity.ReturnType, "RETURN", StringComparison.OrdinalIgnoreCase))
        {
            await ProcessReturnStockAsync(entity, cancellationToken);
        }
        else
        {
            await ProcessExchangeStockAsync(entity, cancellationToken);
        }

        entity.Status = "COMPLETED";
        entity.UpdatedAt = DateTime.UtcNow;
        await _orderReturnRepository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    private async Task<OrderReturnDto> CreateReturnRequestAsync(int customerId, int orderId, OrderReturnDto request, string returnType, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy khách hàng");
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy đơn hàng");

        if (order.IdCustomer != customerId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền tạo yêu cầu cho đơn hàng này");
        }

        if (!string.Equals(order.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Chỉ có thể đổi/trả đơn hàng đã hoàn thành");
        }

        ValidateReturnWindow(order);

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new InvalidOperationException("Danh sách sản phẩm đổi/trả không được để trống");
        }

        var entity = new OrderReturns
        {
            IdOrder = orderId,
            ReturnType = returnType,
            Status = "REQUESTED",
            Reason = request.Reason,
            CreatedByCustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var orderTotal = order.TotalAmount ?? 0m;
        var orderDiscount = order.Discount ?? 0m;
        var refund = 0m;

        foreach (var item in request.Items)
        {
            if (!item.IdOrderDetail.HasValue || !item.Quantity.HasValue || item.Quantity.Value <= 0)
            {
                throw new InvalidOperationException("Chi tiết đổi trả không hợp lệ");
            }

            var detail = await _orderDetailRepository.GetByIdAsync(item.IdOrderDetail.Value, cancellationToken)
                ?? throw new ResourceNotFoundException("Không tìm thấy chi tiết đơn hàng");
            if (detail.IdOrder != orderId)
            {
                throw new InvalidOperationException("Chi tiết đơn hàng không thuộc về order này");
            }

            if (item.Quantity.Value > detail.Quantity)
            {
                throw new InvalidOperationException("Số lượng đổi/trả vượt quá số lượng đã mua");
            }

            var lineSubtotal = detail.Price * item.Quantity.Value;
            var lineRefund = lineSubtotal;
            if (orderTotal > 0 && orderDiscount > 0)
            {
                var ratio = lineSubtotal / orderTotal;
                lineRefund = Math.Max(0m, Math.Round(lineSubtotal - orderDiscount * ratio, 0, MidpointRounding.AwayFromZero));
            }

            refund += lineRefund;
            entity.OrderReturnItems.Add(new OrderReturnItems
            {
                IdOrderDetail = detail.IdOrderDetail,
                Quantity = item.Quantity.Value,
                ExchangeProductId = item.ExchangeProductId,
                ExchangeQuantity = item.ExchangeQuantity ?? item.Quantity.Value,
                LineRefundAmount = lineRefund
            });
        }

        entity.RefundAmount = refund;
        await _orderReturnRepository.AddAsync(entity, cancellationToken);
        await _orderReturnRepository.SaveChangesAsync(cancellationToken);
        var saved = await LoadReturnAsync(entity.IdReturn, cancellationToken);
        return Map(saved);
    }

    private async Task ProcessReturnStockAsync(OrderReturns entity, CancellationToken cancellationToken)
    {
        foreach (var item in entity.OrderReturnItems)
        {
            var product = item.IdOrderDetailNavigation?.IdProductNavigation ?? await _productRepository.GetByIdAsync(item.IdOrderDetailNavigation!.IdProduct, true, cancellationToken)
                ?? throw new ResourceNotFoundException("Không tìm thấy sản phẩm");
            product.StockQuantity += item.Quantity;
            if (string.Equals(product.Status, "OUT_OF_STOCK", StringComparison.OrdinalIgnoreCase) && product.StockQuantity > 0)
            {
                product.Status = "IN_STOCK";
            }

            await _inventoryTransactionRepository.AddAsync(new InventoryTransactions
            {
                IdProduct = product.IdProduct,
                Quantity = item.Quantity,
                TransactionType = "IN",
                ReferenceType = "SALE_RETURN",
                ReferenceId = entity.IdOrder,
                Notes = $"Khách trả hàng - Return #{entity.IdReturn}",
                TransactionDate = DateTime.UtcNow
            }, cancellationToken);
        }

        await _inventoryTransactionRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessExchangeStockAsync(OrderReturns entity, CancellationToken cancellationToken)
    {
        foreach (var item in entity.OrderReturnItems)
        {
            var oldProduct = item.IdOrderDetailNavigation?.IdProductNavigation ?? await _productRepository.GetByIdAsync(item.IdOrderDetailNavigation!.IdProduct, true, cancellationToken)
                ?? throw new ResourceNotFoundException("Không tìm thấy sản phẩm cũ");
            oldProduct.StockQuantity += item.Quantity;
            if (string.Equals(oldProduct.Status, "OUT_OF_STOCK", StringComparison.OrdinalIgnoreCase) && oldProduct.StockQuantity > 0)
            {
                oldProduct.Status = "IN_STOCK";
            }

            await _inventoryTransactionRepository.AddAsync(new InventoryTransactions
            {
                IdProduct = oldProduct.IdProduct,
                Quantity = item.Quantity,
                TransactionType = "IN",
                ReferenceType = "SALE_EXCHANGE",
                ReferenceId = entity.IdOrder,
                Notes = $"Đổi hàng - nhập lại SP cũ (Return #{entity.IdReturn})",
                TransactionDate = DateTime.UtcNow
            }, cancellationToken);

            if (item.ExchangeProductId.HasValue)
            {
                var newProduct = await _productRepository.GetByIdAsync(item.ExchangeProductId.Value, true, cancellationToken)
                    ?? throw new ResourceNotFoundException("Không tìm thấy sản phẩm đổi");
                var quantity = item.ExchangeQuantity ?? item.Quantity;
                if (newProduct.StockQuantity < quantity)
                {
                    throw new InvalidOperationException("Sản phẩm đổi không đủ tồn kho");
                }

                newProduct.StockQuantity -= quantity;
                if (newProduct.StockQuantity <= 0)
                {
                    newProduct.StockQuantity = 0;
                    newProduct.Status = "OUT_OF_STOCK";
                }

                await _inventoryTransactionRepository.AddAsync(new InventoryTransactions
                {
                    IdProduct = newProduct.IdProduct,
                    Quantity = quantity,
                    TransactionType = "OUT",
                    ReferenceType = "SALE_EXCHANGE",
                    ReferenceId = entity.IdOrder,
                    Notes = $"Đổi hàng - xuất SP mới (Return #{entity.IdReturn})",
                    TransactionDate = DateTime.UtcNow
                }, cancellationToken);
            }
        }

        await _inventoryTransactionRepository.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateReturnWindow(Orders order)
    {
        if (!order.ReturnWindowDays.HasValue || order.ReturnWindowDays.Value == 0)
        {
            return;
        }

        var baseTime = order.CompletedAt ?? order.DeliveredAt ?? order.OrderDate;
        if (!baseTime.HasValue)
        {
            return;
        }

        var expireAt = baseTime.Value.AddDays(order.ReturnWindowDays.Value);
        if (DateTime.UtcNow > expireAt)
        {
            throw new InvalidOperationException($"Đơn hàng đã quá hạn đổi/trả ({order.ReturnWindowDays} ngày kể từ khi hoàn thành)");
        }
    }

    private async Task<OrderReturns> LoadReturnAsync(int id, CancellationToken cancellationToken)
    {
        return await _orderReturnRepository.GetByIdWithItemsAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Không tìm thấy yêu cầu đổi/trả với ID: {id}");
    }

    private static OrderReturnDto Map(OrderReturns entity)
    {
        return new OrderReturnDto
        {
            IdReturn = entity.IdReturn,
            OrderId = entity.IdOrder,
            CustomerId = entity.IdOrderNavigation?.IdCustomer,
            CustomerName = entity.CreatedByCustomer?.CustomerName ?? entity.IdOrderNavigation?.IdCustomerNavigation?.CustomerName,
            ReturnType = entity.ReturnType,
            Status = entity.Status,
            Reason = entity.Reason,
            NoteAdmin = entity.NoteAdmin,
            RefundAmount = entity.RefundAmount,
            OrderFinalAmount = entity.IdOrderNavigation?.FinalAmount,
            OrderTotalAmount = entity.IdOrderNavigation?.TotalAmount,
            OrderDiscount = entity.IdOrderNavigation?.Discount,
            OrderShippingFee = entity.IdOrderNavigation?.ShippingFee,
            OrderPromotionCode = entity.IdOrderNavigation?.PromotionCode,
            CreatedByCustomerId = entity.CreatedByCustomerId,
            ProcessedByEmployeeId = entity.ProcessedByEmployeeId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Items = entity.OrderReturnItems.Select(item => new OrderReturnItemDto
            {
                IdReturnItem = item.IdReturnItem,
                IdOrderDetail = item.IdOrderDetail,
                Quantity = item.Quantity,
                ProductName = item.IdOrderDetailNavigation?.ProductNameSnapshot ?? item.IdOrderDetailNavigation?.IdProductNavigation?.ProductName,
                Price = item.IdOrderDetailNavigation?.Price,
                ExchangeProductId = item.ExchangeProductId,
                ExchangeQuantity = item.ExchangeQuantity,
                LineRefundAmount = item.LineRefundAmount
            }).ToList()
        };
    }
}