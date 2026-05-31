namespace PharmaCore.Application.Customers.Requests;

public sealed record ListDeletedCustomersQuery(
    int Page = 1,
    int Limit = 20,
    string? Search = null);
