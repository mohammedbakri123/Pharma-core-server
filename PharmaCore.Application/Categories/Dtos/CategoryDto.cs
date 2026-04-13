namespace PharmaCore.Application.Categories.Dtos;

public sealed record CategoryDto(
    int CategoryId,
    string CategoryName,
    string? CategoryArabicName);
