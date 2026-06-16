using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Common;
using StoreManagement.Dtos.Integration;
using StoreManagement.Dtos.Invoice;
using StoreManagement.Dtos.Purchase;
using StoreManagement.Dtos.Shipment;
using StoreManagement.Services;

namespace StoreManagement.Controllers;

[ApiController]
[Route("api/v1/shipments")]
public class ShipmentController : ControllerBase
{
    private readonly IShipmentService _shipmentService;

    public ShipmentController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<ShipmentDto>>> GetShipmentById(int id, CancellationToken cancellationToken)
    {
        var result = await _shipmentService.GetShipmentByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ShipmentDto>.Success("Lấy thông tin vận đơn thành công", result));
    }

    [HttpGet("order/{orderId:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<ShipmentDto>>> GetShipmentByOrderId(int orderId, CancellationToken cancellationToken)
    {
        var result = await _shipmentService.GetShipmentByOrderIdAsync(orderId, cancellationToken);
        return Ok(ApiResponse<ShipmentDto>.Success("Lấy thông tin vận đơn thành công", result));
    }

    [HttpGet("{id:int}/track")]
    [Authorize(Roles = "ADMIN,EMPLOYEE,CUSTOMER")]
    public async Task<ActionResult<ApiResponse<GhnTrackingDto>>> TrackShipment(int id, CancellationToken cancellationToken)
    {
        var result = await _shipmentService.GetShipmentTrackingAsync(id, cancellationToken);
        return Ok(ApiResponse<GhnTrackingDto>.Success("Lấy thông tin tracking thành công", result));
    }

    [HttpPost("{id:int}/sync-ghn")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<ShipmentDto>>> SyncWithGhn(int id, CancellationToken cancellationToken)
    {
        var result = await _shipmentService.SyncWithGhnAsync(id, cancellationToken);
        return Ok(ApiResponse<ShipmentDto>.Success("Đồng bộ với GHN thành công", result));
    }

    [HttpPost("order/{orderId:int}/create-ghn")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<ShipmentDto>>> CreateGhnShipmentForOrder(int orderId, CancellationToken cancellationToken)
    {
        var result = await _shipmentService.CreateGhnShipmentForOrderAsync(orderId, cancellationToken);
        return Ok(ApiResponse<ShipmentDto>.Success("Tạo vận đơn GHN thành công", result));
    }
}

[ApiController]
[Route("api/v1/import-orders")]
public class ImportOrderController : ControllerBase
{
    private readonly IImportOrderService _importOrderService;
    private readonly ICurrentUserContext _currentUserContext;

    public ImportOrderController(IImportOrderService importOrderService, ICurrentUserContext currentUserContext)
    {
        _importOrderService = importOrderService;
        _currentUserContext = currentUserContext;
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> CreateImportOrder([FromBody] PurchaseOrderDto request, CancellationToken cancellationToken)
    {
        var result = await _importOrderService.CreateImportOrderAsync(request, _currentUserContext.GetEmployeeId(), cancellationToken);
        return Ok(ApiResponse<PurchaseOrderDto>.Success("Tạo đơn nhập hàng thành công", result));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> GetImportOrderById(int id, CancellationToken cancellationToken)
    {
        var result = await _importOrderService.GetImportOrderByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PurchaseOrderDto>.Success("Lấy thông tin đơn nhập hàng thành công", result));
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<PurchaseOrderDto>>>> GetAllImportOrders([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "IdPurchaseOrder", [FromQuery] string sortDirection = "DESC", [FromQuery] string? keyword = null, CancellationToken cancellationToken = default)
    {
        var result = await _importOrderService.GetAllImportOrdersAsync(keyword, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<PurchaseOrderDto>>.Success("Lấy danh sách đơn nhập hàng thành công", result));
    }

    [HttpGet("supplier/{supplierId:int}")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<PurchaseOrderDto>>>> GetImportOrdersBySupplier(int supplierId, [FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "IdPurchaseOrder", [FromQuery] string sortDirection = "DESC", CancellationToken cancellationToken = default)
    {
        var result = await _importOrderService.GetImportOrdersBySupplierAsync(supplierId, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<PurchaseOrderDto>>.Success("Lấy danh sách đơn nhập hàng theo nhà cung cấp thành công", result));
    }

    [HttpGet("history")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<ActionResult<ApiResponse<PageResponse<PurchaseOrderDto>>>> GetImportOrderHistory([FromQuery] string? startDate = null, [FromQuery] string? endDate = null, [FromQuery] int? supplierId = null, [FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] string sortBy = "IdPurchaseOrder", [FromQuery] string sortDirection = "DESC", CancellationToken cancellationToken = default)
    {
        var start = string.IsNullOrWhiteSpace(startDate) ? DateTime.UtcNow.AddMonths(-1) : DateTime.Parse(startDate);
        var end = string.IsNullOrWhiteSpace(endDate) ? DateTime.UtcNow : DateTime.Parse(endDate);
        var result = await _importOrderService.GetImportOrdersByDateRangeAsync(start, end, supplierId, pageNo, pageSize, sortBy, sortDirection, cancellationToken);
        return Ok(ApiResponse<PageResponse<PurchaseOrderDto>>.Success("Lấy lịch sử nhập hàng thành công", result));
    }

    [HttpGet("{id:int}/pdf")]
    [Authorize(Roles = "ADMIN,EMPLOYEE")]
    public async Task<IActionResult> ExportImportOrderToPdf(int id, CancellationToken cancellationToken)
    {
        var bytes = await _importOrderService.ExportImportOrderToPdfAsync(id, cancellationToken);
        return File(bytes, "application/pdf", $"phieu-nhap-hang-{id}.pdf");
    }
}

[ApiController]
[Route("api/v1/admin/invoices")]
[Authorize(Roles = "ADMIN")]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ICurrentUserContext _currentUserContext;

    public InvoiceController(IInvoiceService invoiceService, ICurrentUserContext currentUserContext)
    {
        _invoiceService = invoiceService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet("export")]
    public async Task<ActionResult<ApiResponse<PageResponse<ExportInvoiceDto>>>> GetExportInvoices([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, [FromQuery] string? status = null, CancellationToken cancellationToken = default)
    {
        var result = await _invoiceService.GetExportInvoicesAsync(pageNo, pageSize, fromDate, toDate, status, cancellationToken);
        return Ok(ApiResponse<PageResponse<ExportInvoiceDto>>.Success("Lấy danh sách hóa đơn xuất thành công", result));
    }

    [HttpGet("export/{orderId:int}")]
    public async Task<ActionResult<ApiResponse<ExportInvoiceDto>>> GetExportInvoiceById(int orderId, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.GetExportInvoiceByIdAsync(orderId, cancellationToken);
        return Ok(ApiResponse<ExportInvoiceDto>.Success("Lấy chi tiết hóa đơn xuất thành công", result));
    }

    [HttpPost("export/{orderId:int}/print")]
    public async Task<ActionResult<ApiResponse<ExportInvoiceDto>>> PrintExportInvoice(int orderId, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.PrintExportInvoiceAsync(orderId, _currentUserContext.GetEmployeeId(), cancellationToken);
        return Ok(ApiResponse<ExportInvoiceDto>.Success("In hóa đơn thành công", result));
    }

    [HttpGet("import")]
    public async Task<ActionResult<ApiResponse<PageResponse<ImportInvoiceDto>>>> GetImportInvoices([FromQuery] int pageNo = 1, [FromQuery] int pageSize = 10, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, [FromQuery] int? supplierId = null, CancellationToken cancellationToken = default)
    {
        var result = await _invoiceService.GetImportInvoicesAsync(pageNo, pageSize, fromDate, toDate, supplierId, cancellationToken);
        return Ok(ApiResponse<PageResponse<ImportInvoiceDto>>.Success("Lấy danh sách hóa đơn nhập thành công", result));
    }

    [HttpGet("import/{purchaseOrderId:int}")]
    public async Task<ActionResult<ApiResponse<ImportInvoiceDto>>> GetImportInvoiceById(int purchaseOrderId, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.GetImportInvoiceByIdAsync(purchaseOrderId, cancellationToken);
        return Ok(ApiResponse<ImportInvoiceDto>.Success("Lấy chi tiết hóa đơn nhập thành công", result));
    }

    [HttpPost("import/{purchaseOrderId:int}/print")]
    public async Task<ActionResult<ApiResponse<ImportInvoiceDto>>> PrintImportInvoice(int purchaseOrderId, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.PrintImportInvoiceAsync(purchaseOrderId, _currentUserContext.GetEmployeeId(), cancellationToken);
        return Ok(ApiResponse<ImportInvoiceDto>.Success("In hóa đơn thành công", result));
    }
}