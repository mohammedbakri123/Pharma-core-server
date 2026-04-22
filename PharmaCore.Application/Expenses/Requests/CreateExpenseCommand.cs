namespace PharmaCore.Application.Expenses.Requests;

public sealed record CreateExpenseCommand(
    int? UserId,
    decimal Amount,
    string? Description
);
