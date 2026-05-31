namespace PharmaCore.API.Contracts.Categories;

/// <summary>
/// Request body for creating a new category.
/// </summary>
/// <param name="CategoryName">English name of the category (required).</param>
/// <param name="CategoryArabicName">Arabic name of the category (optional).</param>
public sealed record CreateCategoryRequest(
    string CategoryName,
    string CategoryArabicName);
