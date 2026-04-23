namespace PharmaCore.Application.PurchaseReturns.Requests;

public sealed record CreatePurchaseReturnCommand(
    int PurchaseId,
    int? UserId,
    string? Note,
    List<CreatePurchaseReturnItemCommand> Items,
    RefundPaymentCommand? RefundPayment
);

public sealed record CreatePurchaseReturnItemCommand(
    int PurchaseItemId,
    int BatchId,
    int Quantity,
    decimal UnitPrice
);

public sealed record RefundPaymentCommand(
    int? Method,
    string? Description
);
