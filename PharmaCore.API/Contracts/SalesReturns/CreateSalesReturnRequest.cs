namespace PharmaCore.API.Contracts.SalesReturns;

/// <summary>
/// Request body for creating a new sales return.
/// </summary>
/// <param name="SaleId">Optional original sale ID.</param>
/// <param name="CustomerId">Optional customer ID.</param>
/// <param name="Note">Optional note.</param>
public sealed record CreateSalesReturnRequest(
    int? SaleId,
    int? CustomerId,
    string? Note);