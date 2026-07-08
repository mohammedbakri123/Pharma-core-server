namespace PharmaCore.Application.PurchaseReturns.Requests;

public sealed record CreatePurchaseReturnCommand(
    int PurchaseId,
   int? UserId,
    string? Note
);
