namespace PharmaCore.API.Contracts.Categories;

/// <summary>
/// Request body for updating an existing category.
/// </summary>
/// <param name="CategoryName">Updated English name (optional — omit to keep current).</param>
/// <param name="CategoryArabicName">Updated Arabic name (optional — omit to keep current).</param>
public sealed record UpdateCategoryRequest(
    string? CategoryName,
    string? CategoryArabicName);
