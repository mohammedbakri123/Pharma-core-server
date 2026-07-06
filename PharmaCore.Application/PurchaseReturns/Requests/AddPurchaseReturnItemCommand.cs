namespace PharmaCore.Application.PurchaseReturns.Requests;

public sealed record AddPurchaseReturnItemCommand(int PurchaseReturnId, int PurchaseItemId, int BatchId, int Quantity, decimal UnitPrice);
