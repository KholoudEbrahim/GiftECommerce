using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
    public int FirstItemIndex => (PageNumber - 1) * PageSize + 1;
    public int LastItemIndex => Math.Min(FirstItemIndex + PageSize - 1, TotalCount);
    public bool IsEmpty => Items.Count == 0;
    public int CurrentPageItemCount => Items.Count;
    public static PagedResult<T> Empty(int pageNumber, int pageSize)
    => new()
    {
        Items = new List<T>(),
        TotalCount = 0,
        PageNumber = pageNumber,
        PageSize = pageSize
    };
    public static PagedResult<T> Create(
    List<T> items,
    int totalCount,
    int pageNumber,
    int pageSize)
    => new()
    {
        Items = items,
        TotalCount = totalCount,
        PageNumber = pageNumber,
        PageSize = pageSize
    };
}
