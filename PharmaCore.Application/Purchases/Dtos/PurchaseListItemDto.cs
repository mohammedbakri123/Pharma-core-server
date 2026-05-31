using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Purchases.Dtos;

public sealed record PurchaseListItemDto(
    int PurchaseId,
    int? SupplierId,
    string? SupplierName,
    string? InvoiceNumber,
    decimal TotalAmount,
    PurchaseStatus Status,
    DateTime CreatedAt,
    string? Note
);
