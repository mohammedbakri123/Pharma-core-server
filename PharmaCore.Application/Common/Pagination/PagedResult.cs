namespace PharmaCore.Application.Common.Pagination;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int Limit);
