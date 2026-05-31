namespace PharmaCore.API.Contracts.Purchases;

public sealed record CreatePurchaseReturnRequest(
    string? Note,
    List<CreatePurchaseReturnItemRequest> Items,
    RefundPaymentRequest? RefundPayment
);

public sealed record CreatePurchaseReturnItemRequest(
    int PurchaseItemId,
    int BatchId,
    int Quantity,
    decimal UnitPrice
);

public sealed record RefundPaymentRequest(
    int? Method,
    string? Description
);
