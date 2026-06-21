namespace PharmaCore.Application.Expenses.Requests;

public sealed record ListExpensesQuery(
    int Page = 1,
    int Limit = 20,
    string? Search = null,
    DateTime? From = null,
    DateTime? To = null
);
