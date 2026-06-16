using System.Text;
using StoreManagement.Common;
using StoreManagement.Dtos.Order;
using StoreManagement.Exceptions;
using StoreManagement.Extensions;
using StoreManagement.Models;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public interface IOrderService
{
    Task<OrderDto> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<byte[]> ExportOrderToPdfAsync(int id, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateOrderFromCartAsync(int customerId, OrderDto request, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateOrderDirectlyAsync(int customerId, OrderDto request, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateOrderForCustomerAsync(int? employeeId, OrderDto request, CancellationToken cancellationToken = default);
    Task<PageResponse<OrderDto>> GetMyOrdersAsync(int customerId, string? status, string? keyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<OrderDto> GetMyOrderByIdAsync(int customerId, int orderId, CancellationToken cancellationToken = default);
    Task<OrderDto> CancelOrderAsync(int customerId, int orderId, CancellationToken cancellationToken = default);
    Task<OrderDto> ConfirmDeliveryAsync(int customerId, int orderId, CancellationToken cancellationToken = default);
    Task<PageResponse<OrderDto>> GetAllOrdersAsync(string? status, int? customerId, string? keyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateOrderStatusAsync(int orderId, string newStatus, CancellationToken cancellationToken = default);
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IShippingAddressRepository _shippingAddressRepository;
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
    private readonly IPromotionService _promotionService;
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ISystemSettingService _systemSettingService;

    public OrderService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IEmployeeRepository employeeRepository,
        IShippingAddressRepository shippingAddressRepository,
        ICartRepository cartRepository,
        ICartItemRepository cartItemRepository,
        IProductRepository productRepository,
        IInventoryTransactionRepository inventoryTransactionRepository,
        IPromotionService promotionService,
        IShipmentRepository shipmentRepository,
        ISystemSettingService systemSettingService)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _employeeRepository = employeeRepository;
        _shippingAddressRepository = shippingAddressRepository;
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _promotionService = promotionService;
        _shipmentRepository = shipmentRepository;
        _systemSettingService = systemSettingService;
    }

    public async Task<OrderDto> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(id, cancellationToken);
        return MapOrder(order);
    }

    public async Task<byte[]> ExportOrderToPdfAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await GetOrderByIdAsync(id, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine($"INVOICE #{order.IdOrder}");
        builder.AppendLine($"Customer: {order.CustomerName}");
        builder.AppendLine($"Date: {order.OrderDate:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"Status: {order.Status}");
        builder.AppendLine($"Total: {order.FinalAmount}");
        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    public async Task<OrderDto> CreateOrderFromCartAsync(int customerId, OrderDto request, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy khách hàng");
        var cart = await _cartRepository.GetByCustomerIdAsync(customerId, cancellationToken)
            ?? throw new InvalidOperationException("Giỏ hàng trống");

        if (cart.CartItems is null || cart.CartItems.Count == 0)
        {
            throw new InvalidOperationException("Giỏ hàng trống");
        }

        var lines = new List<(Products Product, int Quantity)>();
        foreach (var item in cart.CartItems)
        {
            var product = await _productRepository.GetByIdAsync(item.IdProduct, false, cancellationToken)
                ?? throw new ResourceNotFoundException($"Sản phẩm không tồn tại với ID: {item.IdProduct}");
            EnsureProductAvailable(product, item.Quantity);
            lines.Add((product, item.Quantity));
        }

        var order = await BuildOrderAsync(customer, null, request, lines, request.PromotionCode, cancellationToken);
        foreach (var cartItem in cart.CartItems.ToList())
        {
            await _cartItemRepository.DeleteAsync(cartItem, cancellationToken);
        }

        await _cartRepository.SaveChangesAsync(cancellationToken);
        return MapOrder(order);
    }

    public async Task<OrderDto> CreateOrderDirectlyAsync(int customerId, OrderDto request, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new ResourceNotFoundException("Không tìm thấy khách hàng");
        if (!request.ProductId.HasValue || !request.Quantity.HasValue || request.Quantity.Value < 1)
        {
            throw new InvalidOperationException("Thiếu thông tin mua ngay");
        }

        var product = await _productRepository.GetByIdAsync(request.ProductId.Value, false, cancellationToken)
            ?? throw new ResourceNotFoundException("Sản phẩm không tồn tại");
        EnsureProductAvailable(product, request.Quantity.Value);

        var order = await BuildOrderAsync(customer, null, request, [(product, request.Quantity.Value)], request.PromotionCode, cancellationToken);
        return MapOrder(order);
    }

    public async Task<OrderDto> CreateOrderForCustomerAsync(int? employeeId, OrderDto request, CancellationToken cancellationToken = default)
    {
        Customers customer;
        if (request.IdCustomer.HasValue)
        {
            customer = await _customerRepository.GetByIdAsync(request.IdCustomer.Value, cancellationToken)
                ?? throw new ResourceNotFoundException($"Không tìm thấy khách hàng với ID: {request.IdCustomer}");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.CustomerPhoneForCreate))
            {
                throw new InvalidOperationException("Số điện thoại khách hàng không được để trống");
            }

            customer = await _customerRepository.GetByPhoneNumberAsync(request.CustomerPhoneForCreate, cancellationToken)
                ?? new Customers
                {
                    CustomerName = request.CustomerNameForCreate ?? "Khách lẻ",
                    PhoneNumber = request.CustomerPhoneForCreate,
                    Address = request.CustomerAddressForCreate,
                    CustomerType = "REGULAR"
                };

            if (customer.IdCustomer == 0)
            {
                await _customerRepository.AddAsync(customer, cancellationToken);
                await _customerRepository.SaveChangesAsync(cancellationToken);
            }
        }

        Employees? employee = null;
        if (employeeId.HasValue)
        {
            employee = await _employeeRepository.GetByIdAsync(employeeId.Value, cancellationToken)
                ?? throw new ResourceNotFoundException($"Không tìm thấy nhân viên với ID: {employeeId}");
        }

        if (request.OrderItems is null || request.OrderItems.Count == 0)
        {
            throw new InvalidOperationException("Danh sách sản phẩm không được để trống");
        }

        var lines = new List<(Products Product, int Quantity)>();
        foreach (var item in request.OrderItems)
        {
            var productId = item.ProductId ?? item.IdProduct;
            var quantity = item.Quantity ?? item.QuantityForCreate ?? 0;
            if (!productId.HasValue || quantity <= 0)
            {
                throw new InvalidOperationException("Chi tiết đơn hàng không hợp lệ");
            }

            var product = await _productRepository.GetByIdAsync(productId.Value, false, cancellationToken)
                ?? throw new ResourceNotFoundException($"Sản phẩm không tồn tại với ID: {productId}");
            EnsureProductAvailable(product, quantity);
            lines.Add((product, quantity));
        }

        var order = await BuildOrderAsync(customer, employee, request, lines, request.PromotionCode, cancellationToken, usePromotionService: false);
        return MapOrder(order);
    }

    public async Task<PageResponse<OrderDto>> GetMyOrdersAsync(int customerId, string? status, string? keyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var page = await _orderRepository.SearchMyOrdersAsync(customerId, NormalizeStatus(status), keyword, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<OrderDto>
        {
            Items = page.Items.Select(MapOrder).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<OrderDto> GetMyOrderByIdAsync(int customerId, int orderId, CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(orderId, cancellationToken);
        if (order.IdCustomer != customerId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xem đơn hàng này");
        }

        return MapOrder(order);
    }

    public async Task<OrderDto> CancelOrderAsync(int customerId, int orderId, CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(orderId, cancellationToken);
        if (order.IdCustomer != customerId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền hủy đơn hàng này");
        }

        if (!string.Equals(order.Status, "PENDING", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Chỉ có thể hủy đơn hàng ở trạng thái PENDING");
        }

        await RestockOrderAsync(order, "Hủy đơn hàng - hoàn trả hàng vào kho", "SALE_ORDER", cancellationToken);
        order.Status = "CANCELED";
        await _orderRepository.SaveChangesAsync(cancellationToken);
        return MapOrder(order);
    }

    public async Task<OrderDto> ConfirmDeliveryAsync(int customerId, int orderId, CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(orderId, cancellationToken);
        if (order.IdCustomer != customerId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xác nhận đơn hàng này");
        }

        if (!string.Equals(order.Status, "CONFIRMED", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Chỉ có thể xác nhận nhận hàng khi đơn hàng ở trạng thái CONFIRMED");
        }

        var now = DateTime.UtcNow;
        order.Status = "COMPLETED";
        order.DeliveredAt = now;
        order.CompletedAt = now;
        order.ReturnWindowDays = await _systemSettingService.GetReturnWindowDaysAsync(cancellationToken);

        var shipment = await _shipmentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        if (shipment is not null)
        {
            shipment.ShippingStatus = "DELIVERED";
            await _shipmentRepository.SaveChangesAsync(cancellationToken);
        }
        else
        {
            await _orderRepository.SaveChangesAsync(cancellationToken);
        }

        return MapOrder(order);
    }

    public async Task<PageResponse<OrderDto>> GetAllOrdersAsync(string? status, int? customerId, string? keyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var page = await _orderRepository.SearchAllAsync(NormalizeStatus(status), customerId, keyword, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<OrderDto>
        {
            Items = page.Items.Select(MapOrder).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, string newStatus, CancellationToken cancellationToken = default)
    {
        var order = await LoadOrderAsync(orderId, cancellationToken);
        var normalized = NormalizeStatus(newStatus) ?? throw new InvalidOperationException("Trạng thái đơn hàng không hợp lệ");

        if (string.Equals(order.Status, "CANCELED", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Không thể cập nhật đơn hàng đã bị hủy");
        }

        if (string.Equals(order.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Không thể cập nhật đơn hàng đã hoàn thành");
        }

        if (normalized == "CANCELED" && !string.Equals(order.Status, "PENDING", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Chỉ có thể hủy đơn hàng ở trạng thái PENDING");
        }

        if (normalized == "CANCELED")
        {
            await RestockOrderAsync(order, "Hủy đơn hàng bởi Admin/Employee - hoàn trả hàng vào kho", "SALE_ORDER", cancellationToken);
        }

        order.Status = normalized;
        if (normalized == "COMPLETED")
        {
            var now = DateTime.UtcNow;
            order.DeliveredAt = now;
            order.CompletedAt = now;
            order.ReturnWindowDays = await _systemSettingService.GetReturnWindowDaysAsync(cancellationToken);
        }

        await _orderRepository.SaveChangesAsync(cancellationToken);
        return MapOrder(order);
    }

    private async Task<Orders> BuildOrderAsync(Customers customer, Employees? employee, OrderDto request, List<(Products Product, int Quantity)> lines, string? promotionCode, CancellationToken cancellationToken, bool usePromotionService = true)
    {
        ShippingAddresses? shippingAddress = null;
        string? shippingSnapshot = null;
        if (request.ShippingAddressId.HasValue)
        {
            shippingAddress = await _shippingAddressRepository.GetByIdAndCustomerIdAsync(request.ShippingAddressId.Value, customer.IdCustomer, cancellationToken)
                ?? throw new ResourceNotFoundException("Không tìm thấy địa chỉ giao hàng");
            shippingSnapshot = $"{shippingAddress.RecipientName}, {shippingAddress.Address}, {shippingAddress.PhoneNumber}";
        }
        else if (!string.IsNullOrWhiteSpace(customer.Address))
        {
            shippingSnapshot = $"{customer.CustomerName}, {customer.Address}, {customer.PhoneNumber}";
        }

        var totalAmount = lines.Sum(x => (x.Product.Price) * x.Quantity);
        decimal discount = request.Discount ?? 0m;

        var shippingFee = request.ShippingFee ?? 0m;
        var finalAmount = Math.Max(0m, totalAmount + shippingFee - discount - (request.ShippingDiscount ?? 0m));

        var order = new Orders
        {
            IdCustomer = customer.IdCustomer,
            IdEmployee = employee?.IdEmployee,
            Status = "PENDING",
            OrderDate = DateTime.UtcNow,
            TotalAmount = totalAmount,
            Discount = discount,
            FinalAmount = finalAmount,
            PaymentMethod = request.PaymentMethod ?? "CASH",
            Notes = request.Notes,
            IdShippingAddress = shippingAddress?.IdShippingAddress,
            ShippingAddressSnapshot = shippingSnapshot,
            PromotionCode = promotionCode,
            ShippingFee = shippingFee,
            ShippingDiscount = request.ShippingDiscount ?? 0m,
            ShippingPromotionCode = request.ShippingPromotionCode
        };

        foreach (var line in lines)
        {
            order.OrderDetails.Add(new OrderDetails
            {
                IdProduct = line.Product.IdProduct,
                Quantity = line.Quantity,
                Price = line.Product.Price,
                ProductNameSnapshot = line.Product.ProductName,
                ProductCodeSnapshot = line.Product.ProductCode,
                ProductImageSnapshot = line.Product.ImageUrl
            });

            if (!string.Equals(order.PaymentMethod, "PAYOS", StringComparison.OrdinalIgnoreCase))
            {
                line.Product.StockQuantity -= line.Quantity;
                if (line.Product.StockQuantity <= 0)
                {
                    line.Product.StockQuantity = 0;
                    line.Product.Status = "OUT_OF_STOCK";
                }
            }
        }

        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        if (!string.Equals(order.PaymentMethod, "PAYOS", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var detail in order.OrderDetails)
            {
                await _inventoryTransactionRepository.AddAsync(new InventoryTransactions
                {
                    IdProduct = detail.IdProduct,
                    Quantity = detail.Quantity,
                    TransactionType = "OUT",
                    ReferenceType = "SALE_ORDER",
                    ReferenceId = order.IdOrder,
                    Notes = employee is null ? "Đơn hàng từ khách hàng" : "Đơn hàng từ nhân viên cho khách hàng",
                    TransactionDate = DateTime.UtcNow
                }, cancellationToken);
            }
            await _inventoryTransactionRepository.SaveChangesAsync(cancellationToken);
        }

        if (shippingAddress is not null)
        {
            var shipment = new Shipments
            {
                IdOrder = order.IdOrder,
                ShippingMethod = "GHN",
                ShippingStatus = "PREPARING",
                TrackingNumber = $"ORDER_{order.IdOrder}"
            };
            await _shipmentRepository.AddAsync(shipment, cancellationToken);
            await _shipmentRepository.SaveChangesAsync(cancellationToken);
        }

        return await LoadOrderAsync(order.IdOrder, cancellationToken);
    }

    private async Task RestockOrderAsync(Orders order, string note, string referenceType, CancellationToken cancellationToken)
    {
        foreach (var detail in order.OrderDetails)
        {
            var product = detail.IdProductNavigation ?? await _productRepository.GetByIdAsync(detail.IdProduct, true, cancellationToken)
                ?? throw new ResourceNotFoundException($"Sản phẩm không tồn tại với ID: {detail.IdProduct}");
            product.StockQuantity += detail.Quantity;
            if (string.Equals(product.Status, "OUT_OF_STOCK", StringComparison.OrdinalIgnoreCase) && product.StockQuantity > 0)
            {
                product.Status = "IN_STOCK";
            }

            await _inventoryTransactionRepository.AddAsync(new InventoryTransactions
            {
                IdProduct = product.IdProduct,
                Quantity = detail.Quantity,
                TransactionType = "IN",
                ReferenceType = referenceType,
                ReferenceId = order.IdOrder,
                Notes = note,
                TransactionDate = DateTime.UtcNow
            }, cancellationToken);
        }

        await _inventoryTransactionRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Orders> LoadOrderAsync(int id, CancellationToken cancellationToken)
    {
        return await _orderRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Đơn hàng không tồn tại với ID: {id}");
    }

    private static void EnsureProductAvailable(Products product, int quantity)
    {
        if (product.IsDelete)
        {
            throw new InvalidOperationException($"Sản phẩm {product.ProductName} không còn khả dụng");
        }

        if (string.Equals(product.Status, "DISCONTINUED", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Sản phẩm {product.ProductName} không còn khả dụng");
        }

        if (product.StockQuantity < quantity)
        {
            throw new InvalidOperationException($"Sản phẩm {product.ProductName} không đủ số lượng. Còn lại: {product.StockQuantity}");
        }
    }

    private static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        var normalized = status.Trim().ToUpperInvariant();
        return normalized is "PENDING" or "CONFIRMED" or "COMPLETED" or "CANCELED" ? normalized : throw new InvalidOperationException($"Trạng thái đơn hàng không hợp lệ: {status}");
    }

    private static OrderDto MapOrder(Orders order)
    {
        return new OrderDto
        {
            IdOrder = order.IdOrder,
            IdCustomer = order.IdCustomer,
            CustomerName = order.IdCustomerNavigation?.CustomerName,
            CustomerAddress = order.IdCustomerNavigation?.Address,
            CustomerPhone = order.IdCustomerNavigation?.PhoneNumber,
            IdEmployee = order.IdEmployee,
            EmployeeName = order.IdEmployeeNavigation?.EmployeeName,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            Discount = order.Discount,
            FinalAmount = order.FinalAmount,
            PaymentMethod = order.PaymentMethod,
            Notes = order.Notes,
            IdShippingAddress = order.IdShippingAddress,
            ShippingAddressSnapshot = order.ShippingAddressSnapshot,
            DeliveredAt = order.DeliveredAt,
            CompletedAt = order.CompletedAt,
            ReturnWindowDays = order.ReturnWindowDays,
            ShippingFee = order.ShippingFee,
            PaymentLinkId = order.PaymentLinkId,
            PromotionCode = order.PromotionCode,
            IdPromotion = order.IdPromotion,
            IdPromotionRule = order.IdPromotionRule,
            ShippingDiscount = order.ShippingDiscount,
            ShippingPromotionCode = order.ShippingPromotionCode,
            IdShippingPromotion = order.IdShippingPromotion,
            OrderDetails = order.OrderDetails.Select(detail => new OrderDetailDto
            {
                IdOrderDetail = detail.IdOrderDetail,
                IdProduct = detail.IdProduct,
                ProductId = detail.IdProduct,
                ProductName = detail.ProductNameSnapshot ?? detail.IdProductNavigation?.ProductName,
                ProductCode = detail.ProductCodeSnapshot ?? detail.IdProductNavigation?.ProductCode,
                Sku = detail.IdProductNavigation?.Sku,
                ProductNameSnapshot = detail.ProductNameSnapshot,
                ProductCodeSnapshot = detail.ProductCodeSnapshot,
                ProductImageSnapshot = detail.ProductImageSnapshot,
                Quantity = detail.Quantity,
                Price = detail.Price,
                Subtotal = detail.Price * detail.Quantity
            }).ToList()
        };
    }
}