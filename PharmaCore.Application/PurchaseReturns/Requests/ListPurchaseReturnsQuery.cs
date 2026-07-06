namespace PharmaCore.Application.PurchaseReturns.Requests;

public sealed record ListPurchaseReturnsQuery(int Page, int Limit, int? PurchaseId);
