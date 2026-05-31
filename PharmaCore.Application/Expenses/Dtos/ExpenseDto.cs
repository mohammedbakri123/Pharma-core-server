namespace PharmaCore.Application.Expenses.Dtos;

public sealed record ExpenseDto(
    int ExpenseId,
    int? UserId,
    decimal Amount,
    string? Description,
    DateTime? CreatedAt
);
