namespace PharmaCore.Application.Expenses.Requests;

public sealed record ListExpensesQuery(
    int Page,
    int Limit,
    DateTime? From,
    DateTime? To
);
