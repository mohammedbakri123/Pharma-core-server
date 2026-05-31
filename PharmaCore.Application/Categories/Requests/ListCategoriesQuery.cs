namespace PharmaCore.Application.Categories.Requests;

public sealed record ListCategoriesQuery(
    int Page = 1,
    int Limit = 20,
    string? Search = null);
