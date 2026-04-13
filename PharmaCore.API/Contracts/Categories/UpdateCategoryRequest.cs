namespace PharmaCore.API.Contracts.Categories;

public sealed record UpdateCategoryRequest(
    string? CategoryName,
    string? CategoryArabicName);
