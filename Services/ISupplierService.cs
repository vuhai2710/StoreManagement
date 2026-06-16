using StoreManagement.Common;
using StoreManagement.Dtos.Supplier;

namespace StoreManagement.Services;

public interface ISupplierService
{
    Task<List<SupplierDto>> GetAllSuppliersAsync(CancellationToken cancellationToken = default);
    Task<PageResponse<SupplierDto>> GetAllSuppliersPaginatedAsync(int pageNo, int pageSize, string sortBy, string sortDirection, string? keyword, CancellationToken cancellationToken = default);
    Task<SupplierDto> GetSupplierByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SupplierDto> CreateSupplierAsync(SupplierDto supplierDto, CancellationToken cancellationToken = default);
    Task<SupplierDto> UpdateSupplierAsync(int id, SupplierDto supplierDto, CancellationToken cancellationToken = default);
    Task DeleteSupplierAsync(int id, CancellationToken cancellationToken = default);
}
