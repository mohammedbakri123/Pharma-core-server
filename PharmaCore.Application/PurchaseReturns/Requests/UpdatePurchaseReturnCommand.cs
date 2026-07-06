namespace PharmaCore.Application.PurchaseReturns.Requests;

public sealed record UpdatePurchaseReturnCommand(int PurchaseReturnId, string? Note);
