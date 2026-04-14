namespace PharmaCore.Application.Categories.Dtos;

/// <summary>
/// Represents a category returned to the client.
/// </summary>
/// <param name="CategoryId">Unique identifier of the category.</param>
/// <param name="CategoryName">English name of the category.</param>
/// <param name="CategoryArabicName">Arabic name of the category.</param>
public sealed record CategoryDto(
    int CategoryId,
    string CategoryName,
    string? CategoryArabicName);
