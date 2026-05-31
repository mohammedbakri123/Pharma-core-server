namespace PharmaCore.API.Contracts.SalesReturns;

/// <summary>
/// Request body for updating a sales return item.
/// </summary>
/// <param name="Quantity">Quantity.</param>
public sealed record UpdateSalesReturnItemRequest(int Quantity);