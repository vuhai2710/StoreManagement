using StoreManagement.Common;
using StoreManagement.Dtos.Supplier;
using StoreManagement.Exceptions;
using StoreManagement.Extensions;
using StoreManagement.Models;
using StoreManagement.Repositories;

namespace StoreManagement.Services;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _supplierRepository;

    public SupplierService(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<List<SupplierDto>> GetAllSuppliersAsync(CancellationToken cancellationToken = default)
    {
        var suppliers = await _supplierRepository.GetAllAsync(cancellationToken);
        return suppliers.Select(MapToDto).ToList();
    }

    public async Task<PageResponse<SupplierDto>> GetAllSuppliersPaginatedAsync(int pageNo, int pageSize, string sortBy, string sortDirection, string? keyword, CancellationToken cancellationToken = default)
    {
        var page = await _supplierRepository.GetPagedAsync(pageNo, pageSize, sortBy, sortDirection, keyword, cancellationToken);
        return new PagedResult<SupplierDto>
        {
            Items = page.Items.Select(MapToDto).ToList(),
            PageNo = page.PageNo,
            PageSize = page.PageSize,
            TotalElements = page.TotalElements,
            TotalPages = page.TotalPages
        }.ToPageResponse();
    }

    public async Task<SupplierDto> GetSupplierByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Nhà cung cấp không tồn tại với ID: {id}");
        return MapToDto(supplier);
    }

    public async Task<SupplierDto> CreateSupplierAsync(SupplierDto supplierDto, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(supplierDto.Email) &&
            await _supplierRepository.GetByEmailAsync(supplierDto.Email, cancellationToken) is not null)
        {
            throw new ConflictException($"Email đã được sử dụng: {supplierDto.Email}");
        }

        var supplier = new Suppliers
        {
            SupplierName = supplierDto.SupplierName,
            Address = supplierDto.Address,
            PhoneNumber = supplierDto.PhoneNumber,
            Email = supplierDto.Email
        };

        await _supplierRepository.AddAsync(supplier, cancellationToken);
        await _supplierRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(supplier);
    }

    public async Task<SupplierDto> UpdateSupplierAsync(int id, SupplierDto supplierDto, CancellationToken cancellationToken = default)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Nhà cung cấp không tồn tại với ID: {id}");

        if (!string.IsNullOrWhiteSpace(supplierDto.Email) &&
            !string.Equals(supplierDto.Email, supplier.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _supplierRepository.GetByEmailAsync(supplierDto.Email, cancellationToken);
            if (existing is not null && existing.IdSupplier != id)
            {
                throw new ConflictException($"Email đã được sử dụng: {supplierDto.Email}");
            }
        }

        supplier.SupplierName = supplierDto.SupplierName;
        supplier.Address = supplierDto.Address;
        supplier.PhoneNumber = supplierDto.PhoneNumber;
        supplier.Email = supplierDto.Email;

        await _supplierRepository.SaveChangesAsync(cancellationToken);
        return MapToDto(supplier);
    }

    public async Task DeleteSupplierAsync(int id, CancellationToken cancellationToken = default)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Nhà cung cấp không tồn tại với ID: {id}");
        await _supplierRepository.DeleteAsync(supplier, cancellationToken);
        await _supplierRepository.SaveChangesAsync(cancellationToken);
    }

    private static SupplierDto MapToDto(Suppliers supplier)
    {
        return new SupplierDto
        {
            IdSupplier = supplier.IdSupplier,
            SupplierName = supplier.SupplierName,
            Address = supplier.Address,
            PhoneNumber = supplier.PhoneNumber,
            Email = supplier.Email,
            CreatedAt = supplier.CreatedAt,
            UpdatedAt = supplier.UpdatedAt
        };
    }
}
