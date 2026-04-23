namespace PharmaCore.Application.PurchaseReturns.Requests;

public sealed record CreatePurchaseReturnCommand(
    int PurchaseId,
    int? UserId,
    string? Note,
    List<CreatePurchaseReturnItemCommand> Items
);

public sealed record CreatePurchaseReturnItemCommand(
    int PurchaseItemId,
    int BatchId,
    int Quantity,
    decimal UnitPrice
);
