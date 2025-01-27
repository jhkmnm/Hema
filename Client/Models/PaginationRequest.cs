namespace Client.Models
{
    public class PaginationRequest
    {
        public int PageSize { get; set; } = 10;
        public int PageIndex { get; set; } = 1;
    }

    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public long TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
} 
