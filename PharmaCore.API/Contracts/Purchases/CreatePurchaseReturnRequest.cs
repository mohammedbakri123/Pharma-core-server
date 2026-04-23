namespace PharmaCore.API.Contracts.Purchases;

public sealed record CreatePurchaseReturnRequest(
    string? Note,
    List<CreatePurchaseReturnItemRequest> Items
);

public sealed record CreatePurchaseReturnItemRequest(
    int PurchaseItemId,
    int BatchId,
    int Quantity,
    decimal UnitPrice
);
