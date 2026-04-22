namespace PharmaCore.Application.Suppliers.Requests;

public sealed record ListSuppliersQuery(
    int Page,
    int Limit,
    string? Search
);
