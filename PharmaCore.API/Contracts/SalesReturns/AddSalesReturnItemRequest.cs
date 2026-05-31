namespace PharmaCore.API.Contracts.SalesReturns;

/// <summary>
/// Request body for adding an item to a sales return.
/// </summary>
/// <param name="SaleItemId">Original sale item ID.</param>
/// <param name="BatchId">Batch ID.</param>
/// <param name="Quantity">Quantity.</param>
/// <param name="UnitPrice">Optional unit price.</param>
public sealed record AddSalesReturnItemRequest(
    int SaleItemId,
    int BatchId,
    int Quantity,
    decimal? UnitPrice);