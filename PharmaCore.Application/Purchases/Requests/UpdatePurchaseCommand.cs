using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Purchases.Requests;

public sealed record UpdatePurchaseCommand(
    int PurchaseId,
    int? SupplierId,
    string? InvoiceNumber,
    string? Note,
    PurchaseStatus? Status
);
