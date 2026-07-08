namespace PharmaCore.Application.PurchaseReturns.Dtos;

public sealed record PurchaseReturnItemDto(
    int PurchaseReturnItemId,
    int PurchaseItemId,
    int BatchId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);