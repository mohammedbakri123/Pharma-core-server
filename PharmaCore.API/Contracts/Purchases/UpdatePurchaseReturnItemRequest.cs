namespace PharmaCore.API.Contracts.Purchases;

/// <summary>
/// Request body for updating a purchase return item.
/// </summary>
/// <param name="Quantity">Quantity.</param>
public sealed record UpdatePurchaseReturnItemRequest(int Quantity);
