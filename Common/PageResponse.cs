namespace StoreManagement.Common;

public class PageResponse<T>
{
    public List<T> Content { get; set; } = [];

    public int PageNo { get; set; }

    public int PageSize { get; set; }

    public long TotalElements { get; set; }

    public int TotalPages { get; set; }

    public bool IsFirst { get; set; }

    public bool IsLast { get; set; }

    public bool HasNext { get; set; }

    public bool HasPrevious { get; set; }

    public bool IsEmpty { get; set; }
}
