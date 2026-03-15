namespace JobHunter.Application.Contracts.Users;

public sealed class ResultPaginationDto<T>
{
    public required MetaDto Meta { get; set; }

    public required IReadOnlyList<T> Result { get; set; }

    public sealed class MetaDto
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public int Pages { get; set; }

        public int Total { get; set; }
    }
}
