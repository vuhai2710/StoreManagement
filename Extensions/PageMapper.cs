using StoreManagement.Common;

namespace StoreManagement.Extensions;

public static class PageMapper
{
    public static PageResponse<T> ToPageResponse<T>(this PagedResult<T> source)
    {
        return new PageResponse<T>
        {
            Content = source.Items.ToList(),
            PageNo = source.PageNo,
            PageSize = source.PageSize,
            TotalElements = source.TotalElements,
            TotalPages = source.TotalPages,
            IsFirst = source.PageNo <= 1,
            IsLast = source.TotalPages == 0 || source.PageNo >= source.TotalPages,
            HasNext = source.TotalPages > 0 && source.PageNo < source.TotalPages,
            HasPrevious = source.PageNo > 1,
            IsEmpty = source.TotalElements == 0
        };
    }
}
