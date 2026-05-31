namespace PharmaCore.Application.Categories.Requests;

public sealed record ListDeletedCategoriesQuery(
    int Page = 1,
    int Limit = 20,
    string? Search = null);
