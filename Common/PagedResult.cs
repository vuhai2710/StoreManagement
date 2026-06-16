namespace StoreManagement.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    public int PageNo { get; init; }

    public int PageSize { get; init; }

    public long TotalElements { get; init; }

    public int TotalPages { get; init; }
}
