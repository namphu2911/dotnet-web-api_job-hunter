namespace JobHunter.Api.Contracts;

public sealed class ListQueryParameters
{
    public int? Page { get; set; }
    public int? Current { get; set; }
    public int? Size { get; set; }
    public int? PageSize { get; set; }
    public string? Filter { get; set; }
    public string? Sort { get; set; }

    public int ResolvePage()
    {
        var page = Page ?? Current ?? 1;
        return page < 1 ? 1 : page;
    }

    public int ResolvePageSize(int defaultSize = 20)
    {
        var pageSize = Size ?? PageSize ?? defaultSize;
        return pageSize < 1 ? defaultSize : pageSize;
    }
}