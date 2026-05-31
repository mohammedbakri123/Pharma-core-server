namespace PharmaCore.Application.Customers.Requests;

public sealed record ListCustomersQuery(
    int Page = 1,
    int Limit = 20,
    string? Search = null);
