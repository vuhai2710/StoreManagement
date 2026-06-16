using System.Text;
using StoreManagement.Common;
using StoreManagement.Dtos.Invoice;
using StoreManagement.Dtos.Purchase;
using StoreManagement.Exceptions;
using StoreManagement.Extensions;
using StoreManagement.Models;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public interface IImportOrderService
{
    Task<PurchaseOrderDto> CreateImportOrderAsync(PurchaseOrderDto purchaseOrderDto, int? employeeId, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDto> GetImportOrderByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PageResponse<PurchaseOrderDto>> GetAllImportOrdersAsync(string? keyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<PageResponse<PurchaseOrderDto>> GetImportOrdersBySupplierAsync(int supplierId, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<PageResponse<PurchaseOrderDto>> GetImportOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, int? supplierId, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default);
    Task<byte[]> ExportImportOrderToPdfAsync(int id, CancellationToken cancellationToken = default);
}

public class ImportOrderService : IImportOrderService
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;

    public ImportOrderService(
        IPurchaseOrderRepository purchaseOrderRepository,
        ISupplierRepository supplierRepository,
        IProductRepository productRepository,
        IEmployeeRepository employeeRepository,
        IInventoryTransactionRepository inventoryTransactionRepository)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _supplierRepository = supplierRepository;
        _productRepository = productRepository;
        _employeeRepository = employeeRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
    }

    public async Task<PurchaseOrderDto> CreateImportOrderAsync(PurchaseOrderDto purchaseOrderDto, int? employeeId, CancellationToken cancellationToken = default)
    {
        if (!purchaseOrderDto.IdSupplier.HasValue)
        {
            throw new InvalidOperationException("ID nhà cung cấp không được để trống");
        }

        _ = await _supplierRepository.GetByIdAsync(purchaseOrderDto.IdSupplier.Value, cancellationToken)
            ?? throw new ResourceNotFoundException($"Nhà cung cấp không tồn tại với ID: {purchaseOrderDto.IdSupplier}");

        if (purchaseOrderDto.ImportOrderDetails is null || purchaseOrderDto.ImportOrderDetails.Count == 0)
        {
            throw new InvalidOperationException("Danh sách sản phẩm không được để trống");
        }

        var order = new PurchaseOrders
        {
            IdSupplier = purchaseOrderDto.IdSupplier,
            IdEmployee = employeeId,
            OrderDate = DateTime.UtcNow
        };

        decimal total = 0m;
        foreach (var item in purchaseOrderDto.ImportOrderDetails)
        {
            if (!item.IdProduct.HasValue || !item.Quantity.HasValue || !item.ImportPrice.HasValue)
            {
                throw new InvalidOperationException("Chi tiết nhập hàng không hợp lệ");
            }

            var product = await _productRepository.GetByIdAsync(item.IdProduct.Value, true, cancellationToken)
                ?? throw new ResourceNotFoundException($"Sản phẩm không tồn tại với ID: {item.IdProduct}");
            var subtotal = item.ImportPrice.Value * item.Quantity.Value;
            total += subtotal;

            order.PurchaseOrderDetails.Add(new PurchaseOrderDetails
            {
                IdProduct = product.IdProduct,
                Quantity = item.Quantity.Value,
                ImportPrice = item.ImportPrice.Value
            });

            product.StockQuantity += item.Quantity.Value;
            if (product.StockQuantity > 0 && string.Equals(product.Status, "OUT_OF_STOCK", StringComparison.OrdinalIgnoreCase))
            {
                product.Status = "IN_STOCK";
            }
        }

        order.TotalAmount = total;
        await _purchaseOrderRepository.AddAsync(order, cancellationToken);
        await _purchaseOrderRepository.SaveChangesAsync(cancellationToken);

        foreach (var detail in order.PurchaseOrderDetails)
        {
            await _inventoryTransactionRepository.AddAsync(new InventoryTransactions
            {
                IdProduct = detail.IdProduct,
                IdEmployee = employeeId,
                Quantity = detail.Quantity,
                TransactionType = "IN",
                ReferenceType = "PURCHASE_ORDER",
                ReferenceId = order.IdPurchaseOrder,
                Notes = $"Nhập hàng - Đơn nhập #{order.IdPurchaseOrder}",
                TransactionDate = DateTime.UtcNow
            }, cancellationToken);
        }

        await _inventoryTransactionRepository.SaveChangesAsync(cancellationToken);
        return await GetImportOrderByIdAsync(order.IdPurchaseOrder, cancellationToken);
    }

    public async Task<PurchaseOrderDto> GetImportOrderByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _purchaseOrderRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Đơn nhập hàng không tồn tại với ID: {id}");
        return Map(order);
    }

    public async Task<PageResponse<PurchaseOrderDto>> GetAllImportOrdersAsync(string? keyword, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var page = await _purchaseOrderRepository.SearchAsync(keyword, null, null, null, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<PurchaseOrderDto>
        {
            Items = page.Items.Select(Map).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<PageResponse<PurchaseOrderDto>> GetImportOrdersBySupplierAsync(int supplierId, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var page = await _purchaseOrderRepository.SearchAsync(null, supplierId, null, null, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<PurchaseOrderDto>
        {
            Items = page.Items.Select(Map).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<PageResponse<PurchaseOrderDto>> GetImportOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, int? supplierId, int pageNo, int pageSize, string sortBy, string sortDirection, CancellationToken cancellationToken = default)
    {
        var page = await _purchaseOrderRepository.SearchAsync(null, supplierId, startDate, endDate, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return new PagedResult<PurchaseOrderDto>
        {
            Items = page.Items.Select(Map).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<byte[]> ExportImportOrderToPdfAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await GetImportOrderByIdAsync(id, cancellationToken);
        return Encoding.UTF8.GetBytes($"IMPORT ORDER #{order.IdImportOrder} - TOTAL {order.TotalAmount}");
    }

    private static PurchaseOrderDto Map(PurchaseOrders order)
    {
        return new PurchaseOrderDto
        {
            IdImportOrder = order.IdPurchaseOrder,
            IdSupplier = order.IdSupplier,
            SupplierName = order.IdSupplierNavigation?.SupplierName,
            SupplierAddress = order.IdSupplierNavigation?.Address,
            SupplierPhone = order.IdSupplierNavigation?.PhoneNumber,
            SupplierEmail = order.IdSupplierNavigation?.Email,
            IdEmployee = order.IdEmployee,
            EmployeeName = order.IdEmployeeNavigation?.EmployeeName ?? (order.IdEmployee.HasValue ? null : "Quản trị viên"),
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            ImportOrderDetails = order.PurchaseOrderDetails.Select(x => new PurchaseOrderDetailDto
            {
                IdImportOrderDetail = x.IdPurchaseOrderDetail,
                IdProduct = x.IdProduct,
                ProductName = x.IdProductNavigation?.ProductName,
                ProductCode = x.IdProductNavigation?.ProductCode,
                Sku = x.IdProductNavigation?.Sku,
                Quantity = x.Quantity,
                ImportPrice = x.ImportPrice,
                Subtotal = x.ImportPrice * x.Quantity
            }).ToList()
        };
    }
}

public interface IInvoiceService
{
    Task<PageResponse<ExportInvoiceDto>> GetExportInvoicesAsync(int pageNo, int pageSize, DateTime? fromDate, DateTime? toDate, string? status, CancellationToken cancellationToken = default);
    Task<ExportInvoiceDto> GetExportInvoiceByIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<ExportInvoiceDto> PrintExportInvoiceAsync(int orderId, int? userId, CancellationToken cancellationToken = default);
    Task<PageResponse<ImportInvoiceDto>> GetImportInvoicesAsync(int pageNo, int pageSize, DateTime? fromDate, DateTime? toDate, int? supplierId, CancellationToken cancellationToken = default);
    Task<ImportInvoiceDto> GetImportInvoiceByIdAsync(int purchaseOrderId, CancellationToken cancellationToken = default);
    Task<ImportInvoiceDto> PrintImportInvoiceAsync(int purchaseOrderId, int? userId, CancellationToken cancellationToken = default);
}

public class InvoiceService : IInvoiceService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;

    public InvoiceService(IOrderRepository orderRepository, IPurchaseOrderRepository purchaseOrderRepository)
    {
        _orderRepository = orderRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
    }

    public async Task<PageResponse<ExportInvoiceDto>> GetExportInvoicesAsync(int pageNo, int pageSize, DateTime? fromDate, DateTime? toDate, string? status, CancellationToken cancellationToken = default)
    {
        var page = await _orderRepository.SearchAllAsync(status, null, null, pageNo, pageSize, "OrderDate", "DESC", cancellationToken);
        var filtered = page.Items.Where(x => (!fromDate.HasValue || x.OrderDate >= fromDate) && (!toDate.HasValue || x.OrderDate <= toDate)).Select(MapExportInvoice).ToList();
        return new PagedResult<ExportInvoiceDto>
        {
            Items = filtered,
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = filtered.Count,
            TotalPages = 1
        }.ToPageResponse();
    }

    public async Task<ExportInvoiceDto> GetExportInvoiceByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Đơn hàng không tồn tại với ID: {orderId}");
        return MapExportInvoice(order);
    }

    public async Task<ExportInvoiceDto> PrintExportInvoiceAsync(int orderId, int? userId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Đơn hàng không tồn tại với ID: {orderId}");
        order.InvoicePrinted = true;
        order.InvoicePrintedAt = DateTime.UtcNow;
        order.InvoicePrintedBy = userId;
        await _orderRepository.SaveChangesAsync(cancellationToken);
        return MapExportInvoice(order);
    }

    public async Task<PageResponse<ImportInvoiceDto>> GetImportInvoicesAsync(int pageNo, int pageSize, DateTime? fromDate, DateTime? toDate, int? supplierId, CancellationToken cancellationToken = default)
    {
        var page = await _purchaseOrderRepository.SearchAsync(null, supplierId, fromDate, toDate, pageNo, pageSize, "OrderDate", "DESC", cancellationToken);
        return new PagedResult<ImportInvoiceDto>
        {
            Items = page.Items.Select(MapImportInvoice).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<ImportInvoiceDto> GetImportInvoiceByIdAsync(int purchaseOrderId, CancellationToken cancellationToken = default)
    {
        var order = await _purchaseOrderRepository.GetByIdWithDetailsAsync(purchaseOrderId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Đơn nhập hàng không tồn tại với ID: {purchaseOrderId}");
        return MapImportInvoice(order);
    }

    public async Task<ImportInvoiceDto> PrintImportInvoiceAsync(int purchaseOrderId, int? userId, CancellationToken cancellationToken = default)
    {
        var order = await _purchaseOrderRepository.GetByIdWithDetailsAsync(purchaseOrderId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Đơn nhập hàng không tồn tại với ID: {purchaseOrderId}");
        order.InvoicePrinted = true;
        order.InvoicePrintedAt = DateTime.UtcNow;
        order.InvoicePrintedBy = userId;
        await _purchaseOrderRepository.SaveChangesAsync(cancellationToken);
        return MapImportInvoice(order);
    }

    private static ExportInvoiceDto MapExportInvoice(Orders order)
    {
        return new ExportInvoiceDto
        {
            OrderId = order.IdOrder,
            CustomerName = order.IdCustomerNavigation?.CustomerName,
            CustomerPhone = order.IdCustomerNavigation?.PhoneNumber,
            ShippingAddressSnapshot = order.ShippingAddressSnapshot,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            Discount = order.Discount,
            ShippingFee = order.ShippingFee,
            FinalAmount = order.FinalAmount,
            InvoicePrinted = order.InvoicePrinted,
            InvoicePrintedAt = order.InvoicePrintedAt,
            InvoicePrintedBy = order.InvoicePrintedBy,
            Items = order.OrderDetails.Select(x => new InvoiceItemDto
            {
                ProductName = x.ProductNameSnapshot ?? x.IdProductNavigation?.ProductName,
                ProductCode = x.ProductCodeSnapshot ?? x.IdProductNavigation?.ProductCode,
                Quantity = x.Quantity,
                Price = x.Price,
                Subtotal = x.Price * x.Quantity
            }).ToList()
        };
    }

    private static ImportInvoiceDto MapImportInvoice(PurchaseOrders order)
    {
        return new ImportInvoiceDto
        {
            PurchaseOrderId = order.IdPurchaseOrder,
            SupplierName = order.IdSupplierNavigation?.SupplierName,
            SupplierPhone = order.IdSupplierNavigation?.PhoneNumber,
            SupplierAddress = order.IdSupplierNavigation?.Address,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            InvoicePrinted = order.InvoicePrinted,
            InvoicePrintedAt = order.InvoicePrintedAt,
            InvoicePrintedBy = order.InvoicePrintedBy,
            Items = order.PurchaseOrderDetails.Select(x => new InvoiceItemDto
            {
                ProductName = x.IdProductNavigation?.ProductName,
                ProductCode = x.IdProductNavigation?.ProductCode,
                Quantity = x.Quantity,
                Price = x.ImportPrice,
                Subtotal = x.ImportPrice * x.Quantity
            }).ToList()
        };
    }
}