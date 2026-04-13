namespace PharmaCore.API.Contracts.Categories;

public sealed record CreateCategoryRequest(
    string CategoryName,
    string? CategoryArabicName);
