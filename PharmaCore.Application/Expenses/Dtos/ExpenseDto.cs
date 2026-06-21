namespace PharmaCore.Application.Expenses.Dtos;

public sealed record ExpenseDto(
    int ExpenseId,
    int? UserId,
    string? UserName,
    decimal Amount,
    string? Description,
    DateTime? CreatedAt
);
