using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Common;

namespace StoreManagement.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortBy, string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query;
        }

        var property = typeof(T).GetProperties()
            .FirstOrDefault(p => string.Equals(p.Name, sortBy, StringComparison.OrdinalIgnoreCase));

        if (property is null)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.Property(parameter, property);
        var conversion = Expression.Convert(propertyAccess, typeof(object));
        var lambda = Expression.Lambda<Func<T, object>>(conversion, parameter);

        return string.Equals(sortDirection, "DESC", StringComparison.OrdinalIgnoreCase)
            ? query.OrderByDescending(lambda)
            : query.OrderBy(lambda);
    }

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int pageNo, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalizedPageNo = pageNo < 1 ? 1 : pageNo;
        var normalizedPageSize = pageSize < 1 ? 10 : pageSize;

        var totalElements = await query.LongCountAsync(cancellationToken);
        var totalPages = totalElements == 0
            ? 0
            : (int)Math.Ceiling(totalElements / (double)normalizedPageSize);

        var items = await query
            .Skip((normalizedPageNo - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            PageNo = normalizedPageNo,
            PageSize = normalizedPageSize,
            TotalElements = totalElements,
            TotalPages = totalPages
        };
    }
}
