namespace JobHunter.Application.Contracts;

public class ResultPaginationDto<T>
{
    public MetaDto Meta { get; set; } = new();
    public IReadOnlyList<T> Result { get; set; } = Array.Empty<T>();

    // Backward compatibility for old consumers.
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public List<T> Items { get; set; } = new();

    public sealed class MetaDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Pages { get; set; }
        public int Total { get; set; }
    }

    public static ResultPaginationDto<TData> Create<TData>(IReadOnlyList<TData> result, int page, int pageSize, int total)
    {
        var pages = pageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new ResultPaginationDto<TData>
        {
            Meta = new ResultPaginationDto<TData>.MetaDto
            {
                Page = page,
                PageSize = pageSize,
                Pages = pages,
                Total = total
            },
            Result = result,
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = result.ToList()
        };
    }
}
