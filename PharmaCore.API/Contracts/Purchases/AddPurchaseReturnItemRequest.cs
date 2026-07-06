namespace PharmaCore.API.Contracts.Purchases;

/// <summary>
/// Request body for adding an item to a purchase return.
/// </summary>
/// <param name="PurchaseItemId">Original purchase item ID.</param>
/// <param name="BatchId">Batch ID.</param>
/// <param name="Quantity">Quantity.</param>
/// <param name="UnitPrice">Unit price.</param>
public sealed record AddPurchaseReturnItemRequest(
    int PurchaseItemId,
    int BatchId,
    int Quantity,
    decimal UnitPrice);
