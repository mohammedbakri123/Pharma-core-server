namespace PharmaCore.Application.Categories.Requests;

public sealed record CreateCategoryCommand(
    string CategoryName,
    string CategoryArabicName);
