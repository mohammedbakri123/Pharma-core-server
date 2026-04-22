namespace PharmaCore.API.Contracts.Expenses;

public sealed record CreateExpenseRequest(
    decimal Amount,
    string? Description
);
