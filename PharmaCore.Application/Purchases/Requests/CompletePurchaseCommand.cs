namespace PharmaCore.Application.Purchases.Requests;

public sealed record CompletePurchaseCommand(int PurchaseId, int? UserId);
