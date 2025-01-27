namespace Service.Models;

/// <summary>
/// 分页请求参数
/// </summary>
public class PaginationRequest
{
    private int _pageSize = 10;
    private int _pageIndex = 1;

    /// <summary>
    /// 每页数量（默认10，最大50）
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Min(Math.Max(1, value), 50);
    }

    /// <summary>
    /// 页码（从1开始）
    /// </summary>
    public int PageIndex
    {
        get => _pageIndex;
        set => _pageIndex = Math.Max(1, value);
    }
}

/// <summary>
/// 分页结果
/// </summary>
public class PaginatedResult<T>
{
    /// <summary>
    /// 当前页数据
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// 总记录数
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }
} 
