namespace PharmaCore.Application.PurchaseReturns.Dtos;

public sealed record PurchaseReturnDto(
    int PurchaseReturnId,
    int? PurchaseId,
    int? SupplierId,
    int? UserId,
    decimal TotalAmount,
    string? Note,
    DateTime CreatedAt,
    IReadOnlyList<PurchaseReturnItemDto> Items
);

public sealed record PurchaseReturnItemDto(
    int PurchaseReturnItemId,
    int PurchaseItemId,
    int BatchId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);
