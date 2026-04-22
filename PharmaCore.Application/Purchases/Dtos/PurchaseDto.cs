using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Purchases.Dtos;

public sealed record PurchaseDto(
    int PurchaseId,
    int? SupplierId,
    string? SupplierName,
    string? InvoiceNumber,
    decimal TotalAmount,
    PurchaseStatus Status,
    DateTime CreatedAt,
    string? Note,
    IReadOnlyList<PurchaseItemDto> Items
);

public sealed record PurchaseItemDto(
    int PurchaseItemId,
    int MedicineId,
    string? MedicineName,
    int BatchId,
    string? BatchNumber,
    int Quantity,
    decimal PurchasePrice,
    decimal SellPrice,
    decimal TotalPrice,
    DateOnly? ExpireDate
);
