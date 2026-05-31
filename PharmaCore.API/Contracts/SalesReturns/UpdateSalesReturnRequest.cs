namespace PharmaCore.API.Contracts.SalesReturns;

/// <summary>
/// Request body for updating a sales return.
/// </summary>
/// <param name="Note">Note.</param>
public sealed record UpdateSalesReturnRequest(string? Note);