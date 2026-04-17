namespace PharmaCore.API.Contracts.Sales;

/// <summary>
/// Request body for updating a sale item quantity.
/// </summary>
/// <param name="Quantity">New quantity.</param>
public sealed record UpdateSaleItemRequest(int Quantity);