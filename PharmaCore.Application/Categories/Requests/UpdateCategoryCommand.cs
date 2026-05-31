namespace PharmaCore.Application.Categories.Requests;

public sealed record UpdateCategoryCommand(
    int CategoryId,
    string? CategoryName,
    string? CategoryArabicName);
