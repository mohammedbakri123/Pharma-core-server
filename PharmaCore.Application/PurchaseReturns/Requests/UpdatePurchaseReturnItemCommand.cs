namespace PharmaCore.Application.PurchaseReturns.Requests;

public sealed record UpdatePurchaseReturnItemCommand(int PurchaseReturnItemId, int Quantity);
