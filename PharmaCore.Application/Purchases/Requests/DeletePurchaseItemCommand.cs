namespace PharmaCore.Application.Purchases.Requests;

public sealed record DeletePurchaseItemCommand(int PurchaseId, int ItemId);
